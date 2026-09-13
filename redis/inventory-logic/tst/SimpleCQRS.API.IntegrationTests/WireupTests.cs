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
        // the event actually being persisted to this aggregate's state stream in Redis
        [Theory, AutoData]
        public async Task when_create_event_then_message_ends_up_in_in_store(Guid id, string itemName)
        {
            var result = await client.PostAsync($"http://localhost:54105/InventoryCommand/Add?name={itemName}&id={id}", null);

            Assert.True(result.IsSuccessStatusCode);
            await Task.Delay(sleepMillisecondsDelay);

            var stateKey = Outbox.StateKey("inventory", $"es:Inventory:{id}");
            var length = await redisConnection.StreamLengthAsync(stateKey);

            Assert.Equal(1, length);
        }
    }
}
