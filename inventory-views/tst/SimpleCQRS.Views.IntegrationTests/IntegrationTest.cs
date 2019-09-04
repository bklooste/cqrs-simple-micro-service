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

namespace SimpleCQRS.Views.IntegrationTest
{

    [Trait("Integration", "Local")]
    public class IntegrationTest : IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();
        readonly IEventStoreConnection eventStoreConnection;
        readonly TimeSpan sleepMillisecondsDelay = TimeSpan.FromMilliseconds(1000);

        public IntegrationTest(IntegrationTestFixture fixture)
        {
            eventStoreConnection = fixture.StoreConnection;
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/items/");

            this.client.BlockGetTillAvailable("IsAvailable");
        }

        [Theory, AutoData]
        public async Task when_create_event_then_its_in_store_in_correct_format(Guid id, string itemName)
        {
            var result = await client.PostAsync($"http://localhost:53107/InventoryCommand/Add?name={itemName}&id={id}", null);
            
            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(sleepMillisecondsDelay);
            var streamName = $"inventory-InventoryItemLogic{id}";
            var streamResult = await eventStoreConnection.ReadStreamEventsForwardAsync(streamName, 0, 1000, true);
            var evnt = streamResult.Events
                .Select(x => Encoding.UTF8.GetString(x.Event.Data))
                .Select(json => (dynamic) JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(json))
                .First();
            Assert.Single(streamResult.Events);

            Assert.Equal(id.ToString(), evnt.id);
        }

        //TODO stream test 
    }
}
