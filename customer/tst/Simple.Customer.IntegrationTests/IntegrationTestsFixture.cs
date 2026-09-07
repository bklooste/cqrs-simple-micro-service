using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

namespace Simple.Customers.IntegrationTest
{
    public class IntegrationTestFixture : IDisposable
    {
        readonly IConfiguration config;

        public int Port => int.Parse(config["CustomerServicePort"]);

        public IntegrationTestFixture()
        {
            var configDefaults = new Dictionary<string, string>
            {
                {"CustomerServicePort", "54104"}
            };

            this.config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDefaults)
                .AddEnvironmentVariables()
                .Build();
        }

        public void Dispose()
        {
        }
    }
}
