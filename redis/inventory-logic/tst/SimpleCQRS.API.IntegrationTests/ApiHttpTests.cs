using System.Net;

using RedisEvents.Producer;

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

    // The event is persisted to the aggregate's own state stream. The read side's propagation
    // is covered by the inventory-views integration tests.
    [Fact]
    public async Task when_create_event_then_its_in_store_in_correct_format()
    {
        var id = Guid.NewGuid();

        await fixture.Scenario()
            .With("id", id)
            .When(Http.Post("Add?name=item-{{id}}&id={{id}}"))
            .ThenStatus(HttpStatusCode.NoContent)
            .Then(async _ =>
            {
                var db = await fixture.Feature<RedisFeature>().DatabaseAsync();
                var stateKey = Outbox.StateKey("inventory", $"es:Inventory:{id}");
                await Eventually.Assert(async () =>
                    Assert.Equal(1, await db.StreamLengthAsync(stateKey)), TimeSpan.FromSeconds(10), "state stream written");
            })
            .RunAsync();
    }
}
