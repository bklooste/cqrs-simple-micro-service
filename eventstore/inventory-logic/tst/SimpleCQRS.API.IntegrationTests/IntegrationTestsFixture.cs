using EventStore.Client;

namespace SimpleCQRS.API.IntegrationTest;

// Runs against the containers started by runtests.cmd: the API on 53104 and EventStore on 2114.
// SvcHttpUrl / ConnectionStrings:EventStoreConnection can be overridden with environment variables.
public sealed class Fixture() : ServiceTestFixture(new ServiceTestOptions
{
    HealthPath = "IsAvailable",
    Config = new Dictionary<string, string?>
    {
        [BaseUrlKey] = "http://localhost:53104/InventoryCommand/",
        ["ConnectionStrings:EventStoreConnection"] = "esdb://admin:changeit@127.0.0.1:2114?tls=false",
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
