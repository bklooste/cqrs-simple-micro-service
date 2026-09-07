using System;
using System.Net.Http;
using System.Threading.Tasks;

using AutoFixture.Xunit2;
using Xunit;

namespace Simple.Customers.IntegrationTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

    [Trait("Integration", "Local")]
    public class ApiHttpTests : IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();

        public ApiHttpTests(IntegrationTestFixture fixture)
        {
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/api/Customers/");

            this.client.BlockGetTillAvailable("IsAvailable");
        }

        // if the service does security we can and should test here.

        [Theory, AutoData]
        public async Task when_create_customer_without_lastname_then_bad_request(string firstName)
        {
            var result = await client.PostAsync($"?firstName={Uri.EscapeDataString(firstName)}&lastName=", null);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}
