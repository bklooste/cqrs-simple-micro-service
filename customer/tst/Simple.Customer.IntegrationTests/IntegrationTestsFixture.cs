namespace Simple.Customers.IntegrationTest;

// Runs against the containers started by runtests.cmd: the customer API on 54104 (backed by Postgres).
// SvcHttpUrl can be overridden with an environment variable.
public sealed class Fixture() : ServiceTestFixture(new ServiceTestOptions
{
    HealthPath = "IsAvailable",
    Config = new Dictionary<string, string?>
    {
        [BaseUrlKey] = "http://localhost:54104/api/Customers/",
    },
});

[CollectionDefinition(nameof(ServiceTestCollection))]
public sealed class ServiceTestCollection : ICollectionFixture<Fixture>;
