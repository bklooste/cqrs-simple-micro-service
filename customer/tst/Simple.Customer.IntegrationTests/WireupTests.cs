using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using AutoFixture.Xunit2;
using Xunit;

namespace Simple.Customers.IntegrationTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

    //We also need to test wiring up
    // this provides nearly all our this calls that code coverage as well as testing configuration
    // Note the actual message correctness is tested in unit tests
    [Trait("Integration", "Local")]
    public class WireupTests : IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();

        public WireupTests(IntegrationTestFixture fixture)
        {
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/api/Customers/");

            this.client.BlockGetTillAvailable("IsAvailable");
        }

        // this test covers the Marten compiled-query wiring, not just the plain CRUD path
        [Theory, AutoData]
        public async Task when_searching_by_lastname_prefix_then_matching_customer_is_returned(string firstName, Guid uniqueSuffix)
        {
            var lastName = "Wireup" + uniqueSuffix.ToString("N");
            var createResult = await client.PostAsync($"?firstName={Uri.EscapeDataString(firstName)}&lastName={lastName}", null);
            Assert.True(createResult.IsSuccessStatusCode);
            var id = JsonSerializer.Deserialize<Guid>(await createResult.Content.ReadAsStringAsync());

            var searchResult = await client.GetAsync($"name={lastName.Substring(0, 4)}");
            Assert.True(searchResult.IsSuccessStatusCode);
            var json = await searchResult.Content.ReadAsStringAsync();

            Assert.Contains(id.ToString(), json);
        }
    }
}
