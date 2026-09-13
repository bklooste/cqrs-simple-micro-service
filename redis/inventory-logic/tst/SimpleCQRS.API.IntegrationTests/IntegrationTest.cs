using System;
using System.Net.Http;
using System.Threading.Tasks;

using AutoFixture.Xunit2;

using RedisEvents.Producer;

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

        // The read side's own propagation of this event onto the topic (consumed by
        // SimpleCQRS.Views' EventProjector) is covered by the inventory-views integration tests;
        // this only asserts the command side's own durable state stream is written correctly.
        [Theory, AutoData]
        public async Task when_create_event_then_its_in_store_in_correct_format(Guid id, string itemName)
        {
            var result = await client.PostAsync($"http://localhost:54105/InventoryCommand/Add?name={itemName}&id={id}", null);

            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(sleepMillisecondsDelay);

            var stateKey = Outbox.StateKey("inventory", $"es:Inventory:{id}");
            var entries = await redisConnection.StreamRangeAsync(stateKey, "0-0", "+", 10);

            Assert.Single(entries);
        }
    }
}
