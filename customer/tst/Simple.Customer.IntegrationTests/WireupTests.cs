using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.Xunit2;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Xunit;

namespace Simple.Customers.IntegrationTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

    //We also need to test wiring up
    // this provides nearly all our this calls that code coverage as well as testing configuration 
    // Note the actual message correctness is tested in unit tests
    [Trait("Integration", "Local")]
    public class WireupTests : IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();
        readonly IEventStoreConnection eventStoreConnection;
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
         public async Task when_create_customer_then_ends_up_in_in_store(Guid id, string itemName)
        {
            var result = await client.PostAsync($"http://localhost:53104/InventoryCommand/Add?name={itemName}&id={id}", null);
            
            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(sleepMillisecondsDelay);
            var streamName = $"inventory-InventoryItemLogic{id}";

            // query postgres 
            var streamResult = await eventStoreConnection.ReadStreamEventsForwardAsync(streamName, 0, 1000, false);
            var evnt = streamResult.Events.First();

            Assert.Single(streamResult.Events);
            Assert.True(evnt.Event.IsJson);
            Assert.Equal("SimpleCQRS.InventoryItemCreated",evnt.Event.EventType);
            var jsonString = Encoding.UTF8.GetString(evnt.Event.Data);

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
            var streamResult = await eventStoreConnection.ReadStreamEventsForwardAsync(streamName, 0, 1000, false);
            var evnt = streamResult.Events.First();

            Assert.Single(streamResult.Events);
            Assert.True(evnt.Event.IsJson);
            Assert.Equal("SimpleCQRS.InventoryItemCreated", evnt.Event.EventType);
            var jsonString = Encoding.UTF8.GetString(evnt.Event.Data);

            dynamic jsonObject = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(jsonString);

            Assert.Equal(id.ToString(), jsonObject.Id);
        }
    }
}
