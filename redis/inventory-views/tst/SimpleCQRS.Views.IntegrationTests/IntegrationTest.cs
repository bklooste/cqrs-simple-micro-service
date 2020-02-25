using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using AutoFixture.Xunit2;
using Newtonsoft.Json;
using StackExchange.Redis;
using Xunit;

namespace SimpleCQRS.Views.IntegrationTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

    [Trait("Integration", "Local")]
    public class IntegrationTest: IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();
        readonly IDatabase eventStoreConnection;
        readonly TimeSpan sleepMillisecondsDelay = TimeSpan.FromMilliseconds(1000);

        public IntegrationTest(IntegrationTestFixture fixture)
        {
            eventStoreConnection = fixture.StoreConnection;
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/");

            this.client.BlockGetTillAvailable("items/");
        }

        [Theory, AutoData]
        public async Task when_create_event_then_its_in_list_view(Guid id, string itemName)
        {
            string json = $"{{\"Id\": \"{id}\",\"Name\": \"{itemName}\", \"Version\": 0}}";
            var streamName = $"inventory-InventoryItemLogic{id}";
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var eventData = new EventData(Guid.NewGuid(), "SimpleCQRS.InventoryItemCreated", true, jsonBytes, null);
            await eventStoreConnection.AppendToStreamAsync(streamName, ExpectedVersion.NoStream, eventData);
            await Task.Delay(sleepMillisecondsDelay*2);

            var response = await client.GetStringAsync("items/");

            //We dont test json convert and .net core mvc conversion, end to end tests will cover that as well.
            // just that the test doest break as schema is changed
            Assert.Contains(id.ToString(), response);
            Assert.Contains(itemName.ToString(), response);
        }

        [Theory, AutoData]
        public async Task when_create_event_then_its_in_item_detail_view(Guid id, string itemName)
        {
            string json = $"{{\"Id\": \"{id}\",\"Name\": \"{itemName}\", \"Version\": 0}}";
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var eventData = new EventData(Guid.NewGuid(), "SimpleCQRS.InventoryItemCreated", true, jsonBytes, null);
            await eventStoreConnection.AppendToStreamAsync($"inventory-InventoryItemLogic{id}", ExpectedVersion.NoStream, eventData);
            await Task.Delay(sleepMillisecondsDelay);

            using var response = await client.GetAsync($"items/{id}");
            var jsonResponse = await client.GetStringAsync($"items/{id}");

            dynamic itemDetail = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(jsonResponse);

            Assert.Equal(id.ToString(), itemDetail.id);
            Assert.Equal(itemName, itemDetail.name);
        }


    }
}
