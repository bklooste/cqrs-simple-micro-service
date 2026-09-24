using System;
using System.Net.Http;
using System.Threading.Tasks;

using AutoFixture.Xunit2;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RedisEvents.EventSourcing;
using RedisEvents.Projections;

using Xunit;

namespace SimpleCQRS.Views.IntegrationTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

    [Trait("Integration", "Local")]
    public class IntegrationTest: IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();
        readonly IEventRepository repository;
        readonly TimeSpan sleepMillisecondsDelay = TimeSpan.FromMilliseconds(1000);

        public IntegrationTest(IntegrationTestFixture fixture)
        {
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/");

            // Publishes exactly the way the command-side service does (same topic, same aggregate
            // name, same wire types) so this exercises the same EventProjector wiring the running
            // service under test uses to build its views.
            var builder = Host.CreateApplicationBuilder();
            builder.Configuration.AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string>
            {
                {"Streams:ConnectionString", "127.0.0.1:6479,allowAdmin=false"},
            });
            builder.AddEventStore("inventory", InventoryEventTypes.Register);
            repository = builder.Build().Services.GetRequiredService<IEventRepository>();

            this.client.BlockGetTillAvailable("items/");
        }

        [Theory, AutoData]
        public async Task when_create_event_then_its_in_list_view(Guid id, string itemName)
        {
            await repository.SaveAsync(TestInventoryItem.Create(id, itemName));
            await Task.Delay(sleepMillisecondsDelay * 2);

            var response = await client.GetStringAsync("items/");

            //We dont test json convert and .net core mvc conversion, end to end tests will cover that as well.
            // just that the test doesnt break as schema is changed
            Assert.Contains(id.ToString(), response);
            Assert.Contains(itemName, response);
        }

        [Theory, AutoData]
        public async Task when_create_event_then_its_in_item_detail_view(Guid id, string itemName)
        {
            await repository.SaveAsync(TestInventoryItem.Create(id, itemName));
            await Task.Delay(sleepMillisecondsDelay * 2);

            var jsonResponse = await client.GetStringAsync($"items/{id}");

            Assert.Contains(id.ToString(), jsonResponse);
            Assert.Contains(itemName, jsonResponse);
        }
    }
}
