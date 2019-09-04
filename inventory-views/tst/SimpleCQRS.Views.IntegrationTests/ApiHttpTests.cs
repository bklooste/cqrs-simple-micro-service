using System;
using System.Net.Http;
using System.Threading.Tasks;

using AutoFixture.Xunit2;
using Xunit;

namespace SimpleCQRS.Views.IntegrationTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

    [Trait("Integration", "Local")]
    public class ApiHttpTests: IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();

        public ApiHttpTests(IntegrationTestFixture fixture)
        {
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/");

            this.client.BlockGetTillAvailable("items/");
        }
        // if the service does security we can and should test here.

        [Theory, AutoData]
        public async Task when_get_unknown_item_then_return_404(Guid id)
        {
            using var result = await client.GetAsync($"items/{id}");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        }


        [Fact]
        public async Task when_get_http2_then_ok()
        {
            using var http2Client = new System.Net.Http.HttpClient
            {
                DefaultRequestVersion = new Version(2, 0),
                BaseAddress = client.BaseAddress
            };

            var result = await http2Client.GetAsync($"items/");

            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }
    }
}
