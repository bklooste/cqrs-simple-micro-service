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
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/InventoryCommand/");
            
            this.client.BlockGetTillAvailable("IsAvailable");
        }

        // if the service does security we can and should test here.

        [Theory, AutoData]
        public async Task when_create_event_then_its_in_store_in_correct_format(Guid id)
        {
            var result = await client.PostAsync($"Add?name=&id={id}", null);
            
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}
