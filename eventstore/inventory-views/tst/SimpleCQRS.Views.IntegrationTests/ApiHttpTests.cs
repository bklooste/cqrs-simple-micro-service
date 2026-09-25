using System.Net;

using System.Net;
using System.Text;

using EventStore.Client;

namespace SimpleCQRS.Views.IntegrationTest;

[Collection(nameof(ServiceTestCollection))]
[Trait("Integration", "Local")]
public class ApiHttpTests(Fixture fixture)
{
    // Appends the event exactly as the command side writes it.
    Task Publish(Guid id, string name)
    {
        var json = $$"""{"Id": "{{id}}","Name": "{{name}}", "Version": 0}""";
        var eventData = new EventData(Uuid.NewUuid(), "SimpleCQRS.InventoryItemCreated", Encoding.UTF8.GetBytes(json));
        return fixture.Store.AppendToStreamAsync($"inventory-InventoryItemLogic{id}", StreamState.NoStream, [eventData]);
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
