namespace SimpleCQRS.Views.IntegrationTest;

// Runs against the docker-compose stack (runtests.cmd): the views API on 54106 and Redis on 6479.
// SvcHttpUrl / Streams:ConnectionString can be overridden with environment variables.
public sealed class Fixture() : ServiceTestFixture(new ServiceTestOptions
{
    HealthPath = "items/",
    Config = new Dictionary<string, string?>
    {
        [BaseUrlKey] = "http://localhost:54106/",
        ["Streams:ConnectionString"] = "127.0.0.1:6479,allowAdmin=false",
    },
});

[CollectionDefinition(nameof(ServiceTestCollection))]
public sealed class ServiceTestCollection : ICollectionFixture<Fixture>;
