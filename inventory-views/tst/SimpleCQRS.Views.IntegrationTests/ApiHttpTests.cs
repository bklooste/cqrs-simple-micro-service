using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.Xunit2;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Xunit;

namespace SimpleCQRS.Views.IntegrationTest
{

    [Trait("Integration", "Local")]
    public class ApiHttpTests 
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();

        public ApiHttpTests(IntegrationTestFixture fixture)
        {
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/InventoryCommand/");

            this.client.BlockGetTillAvailable("items/");
        }
        // if the service does security we can and should test here.

        [Theory, AutoData]
        public async Task when_get_unknown_item_then_return_404(Guid id)
        {
            var result = await client.GetAsync($"items/{id}");
            
            Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}
