using System.Net;

namespace Simple.Customers.IntegrationTest;

[Collection(nameof(ServiceTestCollection))]
[Trait("Integration", "Local")]
public class ApiHttpTests(Fixture fixture)
{
    // if the service does security we can and should test here.

    [Fact]
    public Task when_create_customer_without_lastname_then_bad_request() =>
        fixture.Scenario()
            .When(Http.Post("?firstName=first-{{guid}}&lastName="))
            .ThenStatus(HttpStatusCode.BadRequest)
            .RunAsync();

    // The Get endpoint returns the customer as a json document inside a json string.
    [Fact]
    public Task when_create_customer_then_it_can_be_retrieved()
    {
        var firstName = "first-" + Guid.NewGuid();
        var lastName = "last-" + Guid.NewGuid();

        return fixture.Scenario()
            .When(Http.Post($"?firstName={firstName}&lastName={lastName}"))
            .Capture("id", "$")
            .Then(Http.Get("{{id}}").BodyContains(firstName).BodyContains(lastName))
            .RunAsync();
    }

    [Fact]
    public Task when_update_customer_then_changes_are_persisted()
    {
        var newFirstName = "newfirst-" + Guid.NewGuid();
        var newLastName = "newlast-" + Guid.NewGuid();

        return fixture.Scenario()
            .Given(Http.Post("?firstName=first-{{guid}}&lastName=last-{{guid}}"))
            .Capture("id", "$")
            .When(Http.Put($"{{{{id}}}}/?firstName={newFirstName}&lastName={newLastName}"))
            .Then(Http.Get("{{id}}").BodyContains(newFirstName).BodyContains(newLastName))
            .RunAsync();
    }

    [Fact]
    public Task when_update_unknown_customer_then_bad_request() =>
        fixture.Scenario()
            .When(Http.Put("{{guid}}/?firstName=first&lastName=last"))
            .ThenStatus(HttpStatusCode.BadRequest)
            .RunAsync();

    // Covers the Marten compiled-query wiring, not just the plain CRUD path.
    [Fact]
    public Task when_searching_by_lastname_prefix_then_matching_customer_is_returned() =>
        fixture.Scenario()
            .With("lastName", "Wireup" + Guid.NewGuid().ToString("N"))
            .Given(Http.Post("?firstName=first&lastName={{lastName}}"))
            .Capture("id", "$")
            .When(Http.Get("name=Wire"))
            .Then(Http.Get("name={{lastName}}").Matches("""[ { "id": "{{id}}" } ]"""))
            .RunAsync();
}
