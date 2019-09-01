using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.Xunit2;
using EventStore.ClientAPI;
using Xunit;

namespace SimpleCQRS.API.Test
{
    //We have only 1 external dependency , writing to the event store , which is mainly covered by wire up 
    [Trait("Integration", "Local")]
    public class IntegrationTest : IClassFixture<EventStoreFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();
        readonly IEventStoreConnection eventStoreConnection;

        public IntegrationTest(EventStoreFixture fixture)
        {
            eventStoreConnection = fixture.StoreConnection;
        }

        extention http client
        public static async Task RunUntilPassed(Func<Task<bool>> isOk)
        {
            for (var i = 0; i < 20; i++)
            {
                if (i == 20)
                    throw new Exception($"20 tries giving up");
                try
                {
                    if (await isOk())
                        break;
                }
                catch (HttpRequestException)
                {
                }

                Thread.Sleep(i * i * 200);
            }
        }


        [Fact]
        public async Task Test1()
        {
            await RunUntilPassed(async () =>
            {
                var status = await client.GetStringAsync($"http://localhost:9092/livehole/read/api/v1/Queries/GetStatus");
                if (int.Parse(status) == 4)
                    return true;

                return false;
            });

        }

        [Theory, AutoData]
        public async Task when_receive_broadcast_then_its_in_plan_stream(Guid siteId, Guid planId)
        {
            var broadcastV2 = fixture.Build<DrillPlanBroadcastv2B>().With(x => x.Rules, value: null).Create();
            broadcastV2.SiteId = siteId;
            broadcastV2.DrillPlanId = planId;
            var caller = new CosmosCaller(siteId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            await routerConnection.PublishToStreamAsync("blastdesign", caller, broadcastV2);

            await Task.Delay(SleepMillisecondsDelay + SleepMillisecondsDelay);
            var planStream = $"livehole-PlanBL.{planId:N}";
            var streamResult = await liveHoleConnection.ReadStreamEventsForwardAsync(planStream, 0, 1000, false);

            var evnt = streamResult.Events.First(x => x.Event.EventType == "Cosmos.HoleDesign.Contract.DrillPlanBroadcastv2B");

            Assert.Equal("Cosmos.HoleDesign.Contract.DrillPlanBroadcastv2B", evnt.Event.EventType);
            Assert.True(evnt.Event.Data.Length > 0);
        }


        [Theory, AutoData]
        public async Task when_create_plan_then_its_in_router_livehole_stream(string planName, Guid planId)
        {
            var result = await client.PostAsync($"http://localhost:9092/livehole/write/api/v1/Plan/CreateDrillPlan?newDrillPlanName={planName}&planId={planId}", null);
            Assert.True(result.IsSuccessStatusCode);

            await Task.Delay(SleepMillisecondsDelay);
            var streamResult = await routerConnection.ReadStreamEventsForwardAsync("livehole", 0, 1000, true);
            var evnts = streamResult.Events
                .Where(x => x.Event.EventType == "Cosmos.LiveHoles.PlanCreatedv2")
                .Select(x => Encoding.UTF8.GetString(x.Event.Data))
                .Select(JsonConvert.DeserializeObject<PlanCreatedv2>);
            Assert.Contains(evnts, x => x.PlanId == planId);
        }

        [Theory, AutoData]
        public async Task given_broadcast_plan_when_add_hole_then_its_in_router_livehole_stream(string planName, Guid planId, Guid holeId)
        {
            var result = await client.PostAsync($"http://localhost:9092/livehole/write/api/v1/Plan/CreateDrillPlan?newDrillPlanName={planName}&planId={planId}", null);
            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(500);
            result = await client.PostAsync($"http://localhost:9092/livehole/write/api/v1/Hole/CreateHole?holeGuid={holeId}&drillPlanName={planName}&holeIdentifier=a1", null);
            await Task.Delay(SleepMillisecondsDelay);


            Assert.True(result.IsSuccessStatusCode);
            var streamResult = await routerConnection.ReadStreamEventsForwardAsync("livehole", 0, 1000, true);
            var evnts = streamResult.Events
                .Where(x => x.Event.EventType == "Cosmos.LiveHoles.HoleCreatedWithoutLocationv1")
                .Select(x => Encoding.UTF8.GetString(x.Event.Data))
                .Select(JsonConvert.DeserializeObject<HoleCreatedWithoutLocationv1>)
                .ToList();

            Assert.Contains(evnts, x => x.PlanName == planName);
            Assert.Contains(evnts, x => x.HoleId == holeId);
        }

        [Theory, AutoData]
        public async Task when_receive_delete_blast_then_delete_message_is_in_plan_stream(Blast.Contracts.Models.Events.BlastDeletedV1 blastDeletedV1)
        {
            var siteId = blastDeletedV1.SiteId;
            var planId = Guid.NewGuid();
            blastDeletedV1.PlanIds = new Guid[] { planId };
            var planStreamName = $"livehole-PlanBL.{planId:N}";

            var plancreated = new PlanCreatedv2() { PlanId = planId, SiteId = siteId, Time = DateTime.UtcNow - TimeSpan.FromMinutes(1), Name = "test", Designed = false, Id = Guid.NewGuid() };
            var caller = new CosmosCaller(siteId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            await liveHoleConnection.PublishToStreamAsync(planStreamName, caller, plancreated);  //TODO publish without migration ?

            await SubscriberTestFixture.RunUntilPassed(async () =>
            {
                var result = await holeEventsConnection.ReadStreamEventsForwardAsync(planStreamName, 0, 1000, true);
                return result.Events.Select(x => x.Event.EventType).Contains("Cosmos.HoleEvents.PlanCreatedV2");
            });


            await routerConnection.PublishToStreamAsync("blast", caller, blastDeletedV1);


            await SubscriberTestFixture.RunUntilPassed(async () =>
            {
                var result = await holeEventsConnection.ReadStreamEventsForwardAsync(planStreamName, 0, 1000, true);
                return result.Events.Select(x => x.Event.EventType).Contains("Cosmos.HoleEvents.PlanDeletedV1");
            });
            var streamResult = await holeEventsConnection.ReadStreamEventsForwardAsync(planStreamName, 0, 1000, false);
            var evnt = streamResult.Events.First(x => x.Event.EventType == "Cosmos.HoleEvents.PlanDeletedV1");
            Assert.True(evnt.Event.Data.Length > 0);
        }
    }
}
