using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.Xunit2;
using EventStore.Client;
using Newtonsoft.Json;
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
        readonly EventStoreClient eventStoreConnection;
        readonly TimeSpan sleepMillisecondsDelay = TimeSpan.FromMilliseconds(1000);

        public WireupTests(IntegrationTestFixture fixture)
        {
            eventStoreConnection = fixture.StoreConnection;
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/InventoryCommand/");

            this.client.BlockGetTillAvailable("IsAvailable");
        }

        // this test covers
        // json convert called the correct message is written to the right place
        [Theory, AutoData]
         public async Task when_create_event_then_message_ends_up_in_in_store(Guid id, string itemName)
        {
            var result = await client.PostAsync($"http://localhost:53104/InventoryCommand/Add?name={itemName}&id={id}", null);

            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(sleepMillisecondsDelay);
            var streamName = $"inventory-InventoryItemLogic{id}";
            var events = new List<ResolvedEvent>();
            await foreach (var storeEvent in eventStoreConnection.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, 1000, resolveLinkTos: false))
                events.Add(storeEvent);
            var evnt = events.First();

            Assert.Single(events);
            Assert.Equal("application/json", evnt.Event.ContentType);
            Assert.Equal("SimpleCQRS.InventoryItemCreated",evnt.Event.EventType);
            var jsonString = Encoding.UTF8.GetString(evnt.Event.Data.Span);

            dynamic jsonObject = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(jsonString);

            Assert.Equal(id.ToString(), jsonObject.Id);
        }

        [Theory, AutoData]
        public async Task when_create_rename_event_then_message_ends_up_in_in_store(Guid id, string itemName)
        {
            var result = await client.PostAsync($"http://localhost:53104/InventoryCommand/Add?name={itemName}&id={id}", null);

            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(sleepMillisecondsDelay);
            var streamName = $"inventory-InventoryItemLogic{id}";
            var events = new List<ResolvedEvent>();
            await foreach (var storeEvent in eventStoreConnection.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, 1000, resolveLinkTos: false))
                events.Add(storeEvent);
            var evnt = events.First();

            Assert.Single(events);
            Assert.Equal("application/json", evnt.Event.ContentType);
            Assert.Equal("SimpleCQRS.InventoryItemCreated", evnt.Event.EventType);
            var jsonString = Encoding.UTF8.GetString(evnt.Event.Data.Span);

            dynamic jsonObject = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(jsonString);

            Assert.Equal(id.ToString(), jsonObject.Id);
        }
    }
}
