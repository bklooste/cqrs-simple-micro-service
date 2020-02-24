using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.Xunit2;

using Newtonsoft.Json;
using StackExchange.Redis;
using Xunit;

namespace SimpleCQRS.API.IntegrationTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

    //We also need to test wiring up
    // this provides nearly all our this calls that code coverage as well as testing configuration 
    // Note the actual message correctness is tested in unit tests
    [Trait("Integration", "Local")]
    public class WireupTests : IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();
        readonly IDatabase redisConnection;
        readonly TimeSpan sleepMillisecondsDelay = TimeSpan.FromMilliseconds(1000);

        public WireupTests(IntegrationTestFixture fixture)
        {
            redisConnection = fixture.StoreConnection;
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/InventoryCommand/");

            this.client.BlockGetTillAvailable("IsAvailable");
        }

        // this test covers
        // json convert called the correct message is written to the right place 
        [Theory, AutoData]
         public async Task when_create_event_then_message_ends_up_in_in_store(Guid id, string itemName)
        {
            var result = await client.PostAsync($"http://localhost:54105/InventoryCommand/Add?name={itemName}&id={id}", null);
            
            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(sleepMillisecondsDelay);
            var streamName = $"inventory-InventoryItemLogic{id}";
            var streamResult = await redisConnection.StreamRangeAsync(streamName, "0-0", "+", 1000);
            var evntJson = streamResult
                .Select(x => x.Values)
                .Select(x => Encoding.UTF8.GetString(x.First(field => field.Name == "msg").Value))
                .Select(json => (dynamic)JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(json))
                .ToList();


            Assert.Single(streamResult);
            var evnt = streamResult.First().Values;
            Assert.Equal("SimpleCQRS.InventoryItemCreated",evnt.First(field => field.Name == "type").Value);
            var jsonString = Encoding.UTF8.GetString(evnt.First(field => field.Name == "msg").Value);

            dynamic jsonObject = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(jsonString);

            Assert.Equal(id.ToString(), jsonObject.Id);
        }

        //TODO
        //[Theory, AutoData]
        //public async Task when_create_rename_event_then_message_ends_up_in_in_store(Guid id, string itemName)
        //{
        //    var result = await client.PostAsync($"http://localhost:54105/InventoryCommand/Add?name={itemName}&id={id}", null);

        //    Assert.True(result.IsSuccessStatusCode);
        //    await Task.Delay(sleepMillisecondsDelay);
        //    var streamName = $"inventory-InventoryItemLogic{id}";
        //    var streamResult = await redisConnection.StreamReadAsync(streamName, "0-0");
        //    var evnts = streamResult
        //        .Select(x => x.Values)
        //        .Select(x => Encoding.UTF8.GetString(x.First(field => field.Name == "msg").Value))
        //        .Select(json => (dynamic)JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(json))
        //        .ToList();



        //    //Assert.True(evnt.Event.IsJson);
        //    Assert.Contains("SimpleCQRS.InventoryItemRenamed", evnts.Where( x=> x...Event.EventType);
        //    //var jsonString = Encoding.UTF8.GetString(evnt.Event.Data);

        //    //dynamic jsonObject = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(jsonString);

        //    //Assert.Equal(id.ToString(), jsonObject.Id);
        //}
    }
}
