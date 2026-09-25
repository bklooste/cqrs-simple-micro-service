using System.Net;
using System.Text;
using System.Text.Json;

using EventStore.Client;

namespace SimpleCQRS.API.IntegrationTest;

[Collection(nameof(ServiceTestCollection))]
[Trait("Integration", "Local")]
public class ApiHttpTests(Fixture fixture)
{
    [Fact]
    public Task when_add_with_empty_name_then_bad_request() =>
        fixture.Scenario()
            .When(Http.Post("Add?name=&id={{guid}}"))
            .ThenStatus(HttpStatusCode.BadRequest)
            .RunAsync();

    // Wire up: the event is written as json, with the right type, to the aggregate's stream.
    [Fact]
    public Task when_create_event_then_message_ends_up_in_in_store()
    {
        var id = Guid.NewGuid();

        return fixture.Scenario()
            .With("id", id)
            .When(Http.Post("Add?name=item-{{id}}&id={{id}}"))
            .ThenStatus(HttpStatusCode.NoContent)
            .Then(async _ => await Eventually.Assert(async () =>
            {
                var events = await ReadAsync(Direction.Forwards, $"inventory-InventoryItemLogic{id}", 1000, resolveLinkTos: false);

                var evnt = Assert.Single(events).Event;
                Assert.Equal("application/json", evnt.ContentType);
                Assert.Equal("SimpleCQRS.InventoryItemCreated", evnt.EventType);
                Assert.Equal(id.ToString(), JsonDocument.Parse(evnt.Data).RootElement.GetProperty("Id").GetString());
            }, TimeSpan.FromSeconds(10), "event stored"))
            .RunAsync();
    }

    // The category stream ($ce-inventory) is what feeds the read model.
    [Fact]
    public Task when_create_event_then_its_in_category_stream_for_read_model()
    {
        var id = Guid.NewGuid();

        return fixture.Scenario()
            .With("id", id)
            .When(Http.Post("Add?name=item-{{id}}&id={{id}}"))
            .ThenStatus(HttpStatusCode.NoContent)
            .Then(async _ => await Eventually.Assert(async () =>
            {
                var events = await ReadAsync(Direction.Backwards, "$ce-inventory", 20, resolveLinkTos: true);

                Assert.Contains(events, e => Encoding.UTF8.GetString(e.Event.Data.Span).Contains(id.ToString()));
            }, TimeSpan.FromSeconds(10), "event in category stream"))
            .RunAsync();
    }

    async Task<List<ResolvedEvent>> ReadAsync(Direction direction, string stream, long count, bool resolveLinkTos)
    {
        var events = new List<ResolvedEvent>();
        var start = direction == Direction.Forwards ? StreamPosition.Start : StreamPosition.End;
        try
        {
            await foreach (var e in fixture.Store.ReadStreamAsync(direction, stream, start, count, resolveLinkTos))
                events.Add(e);
        }
        catch (StreamNotFoundException)
        {
        }
        return events;
    }
}
