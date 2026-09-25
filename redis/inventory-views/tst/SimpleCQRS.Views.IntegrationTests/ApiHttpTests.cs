using System.Net;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RedisEvents.EventSourcing;
using RedisEvents.Projections;

namespace SimpleCQRS.Views.IntegrationTest;

[Collection(nameof(ServiceTestCollection))]
[Trait("Integration", "Local")]
public class ApiHttpTests(Fixture fixture)
{
    // Publishes exactly the way the command-side service does (same topic, same aggregate
    // name, same wire types) so this exercises the same EventProjector wiring the running
    // service under test uses to build its views.
    Task Publish(Guid id, string name)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Streams:ConnectionString"] = fixture.Config["Streams:ConnectionString"],
        });
        builder.AddEventStore("inventory", InventoryEventTypes.Register);
        var repository = builder.Build().Services.GetRequiredService<IEventRepository>();
        return repository.SaveAsync(TestInventoryItem.Create(id, name)).AsTask();
    }

    [Fact]
    public Task when_get_unknown_item_then_return_404() =>
        fixture.Scenario()
            .When(Http.Get("items/{{guid}}"))
            .ThenStatus(HttpStatusCode.NotFound)
            .RunAsync();

    [Fact]
    public async Task when_get_http2_then_ok()
    {
        using var http2Client = new HttpClient
        {
            DefaultRequestVersion = new Version(2, 0),
            BaseAddress = new Uri(fixture.BaseUrl),
        };

        await Eventually.Assert(async () =>
        {
            using var result = await http2Client.GetAsync("items/");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }, TimeSpan.FromSeconds(10), "items list served over http/2");
    }

    // We dont test json conversion here (end to end tests cover that) - just that the test
    // doesnt break as the schema is changed.
    [Fact]
    public Task when_create_event_then_its_in_list_view()
    {
        var id = Guid.NewGuid();
        var name = "item-" + Guid.NewGuid();

        return fixture.Scenario()
            .With("id", id)
            .With("name", name)
            .When(_ => Publish(id, name))
            .Then(Http.Get("items/").Eventually().Matches("""[ { "id": "{{id}}", "name": "{{name}}" } ]"""))
            .RunAsync();
    }

    [Fact]
    public Task when_create_event_then_its_in_item_detail_view()
    {
        var id = Guid.NewGuid();
        var name = "item-" + Guid.NewGuid();

        return fixture.Scenario()
            .With("id", id)
            .With("name", name)
            .When(_ => Publish(id, name))
            .Then(Http.Get("items/{{id}}").Eventually().Matches("""{ "id": "{{id}}", "name": "{{name}}" }"""))
            .RunAsync();
    }
}
