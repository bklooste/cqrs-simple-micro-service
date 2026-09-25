using EventStore.Client;

namespace SimpleCQRS.Views.IntegrationTest;

// Runs against the containers started by runtests.cmd: the views API on 53105 and EventStore on 2115.
// SvcHttpUrl / ConnectionStrings:EventStoreConnection can be overridden with environment variables.
public sealed class Fixture() : ServiceTestFixture(new ServiceTestOptions
{
    HealthPath = "items/",
    Config = new Dictionary<string, string?>
    {
        [BaseUrlKey] = "http://localhost:53105/",
        ["ConnectionStrings:EventStoreConnection"] = "esdb://admin:changeit@127.0.0.1:2115?tls=false",
    },
})
{
    EventStoreClient? store;

    public EventStoreClient Store => store ??= new EventStoreClient(EventStoreClientSettings.Create(Config["ConnectionStrings:EventStoreConnection"]!));

    public override async ValueTask DisposeAsync()
    {
        store?.Dispose();
        await base.DisposeAsync();
    }
}

[CollectionDefinition(nameof(ServiceTestCollection))]
public sealed class ServiceTestCollection : ICollectionFixture<Fixture>;
