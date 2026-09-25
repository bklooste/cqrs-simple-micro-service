namespace SimpleCQRS.API.IntegrationTest;

// Runs against the docker-compose stack (runtests.cmd): the API on 54105 and Redis on 6479.
// SvcHttpUrl / Streams:ConnectionString can be overridden with environment variables.
public sealed class Fixture() : ServiceTestFixture(new ServiceTestOptions
{
    HealthPath = "IsAvailable",
    Config = new Dictionary<string, string?>
    {
        [BaseUrlKey] = "http://localhost:54105/InventoryCommand/",
        ["Streams:ConnectionString"] = "127.0.0.1:6479,allowAdmin=false",
    },
    Features = [new RedisFeature()],
});

[CollectionDefinition(nameof(ServiceTestCollection))]
public sealed class ServiceTestCollection : ICollectionFixture<Fixture>;
