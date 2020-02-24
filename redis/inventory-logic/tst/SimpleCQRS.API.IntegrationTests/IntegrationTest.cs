using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using AutoFixture.Xunit2;

using Newtonsoft.Json;
using StackExchange.Redis;
using Xunit;

namespace SimpleCQRS.API.IntegrationTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]


    [Trait("Integration", "Local")]
    public class IntegrationTest : IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();
        readonly IDatabase redisConnection;
        readonly TimeSpan sleepMillisecondsDelay = TimeSpan.FromMilliseconds(1000);

        public IntegrationTest(IntegrationTestFixture fixture)
        {
            redisConnection = fixture.StoreConnection;
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/InventoryCommand/");

            this.client.BlockGetTillAvailable("IsAvailable");
        }

        [Fact]
        public void when_receive_item_then_it_has_exchange_rate_from_feed()
        {
            //TODO We have only 1 external dependency , writing to the event store , which is mainly covered by wire up but leave one test for expansion 
        }

        [Theory, AutoData]
        public async Task when_create_event_then_its_in_store_in_correct_format(Guid id, string itemName)
        {
            var result = await client.PostAsync($"http://localhost:54105/InventoryCommand/Add?name={itemName}&id={id}", null);
            
            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(sleepMillisecondsDelay);
            var streamName = $"inventory-InventoryItemLogic{id}";
            var streamResult = await redisConnection.StreamReadAsync(streamName, "0-0");  
            var evnt = streamResult
                .Select(x => x.Values)
                .Select(x => Encoding.UTF8.GetString(x.First( field => field.Name == "msg").Value))
                .Select(json => (dynamic) JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(json))
                .First();
            Assert.Single(streamResult);

            Assert.Equal(id.ToString(), evnt.Id);
        }

        [Theory, AutoData]
        public async Task when_create_event_then_its_in_category_stream_for_read_model(Guid id, string itemName)
        {
            var result = await client.PostAsync($"http://localhost:54105/InventoryCommand/Add?name={itemName}&id={id}", null);

            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(sleepMillisecondsDelay);
            var streamName = $"$ce-inventory";
            var streamResult = await redisConnection.StreamRangeAsync(streamName, "0-0" , "+",  20,  Order.Descending);
            var streamNames = streamResult
                .Select(x => x.Values)
                .Select(x => Encoding.UTF8.GetString(x.First(field => field.Name == "stream").Value))
                .ToList();

            Assert.Contains(streamNames, json => json.Contains(id.ToString()));
        }

    }
}
