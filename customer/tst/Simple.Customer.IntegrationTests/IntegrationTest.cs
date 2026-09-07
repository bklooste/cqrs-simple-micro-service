using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using AutoFixture.Xunit2;
using Xunit;

namespace Simple.Customers.IntegrationTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

    [Trait("Integration", "Local")]
    public class IntegrationTest : IClassFixture<IntegrationTestFixture>
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();

        public IntegrationTest(IntegrationTestFixture fixture)
        {
            client.BaseAddress = new Uri($"http://localhost:{fixture.Port}/api/Customers/");

            this.client.BlockGetTillAvailable("IsAvailable");
        }

        [Theory, AutoData]
        public async Task when_create_customer_then_it_can_be_retrieved(string firstName, string lastName)
        {
            var createResult = await client.PostAsync($"?firstName={Uri.EscapeDataString(firstName)}&lastName={Uri.EscapeDataString(lastName)}", null);
            Assert.True(createResult.IsSuccessStatusCode);
            var id = JsonSerializer.Deserialize<Guid>(await createResult.Content.ReadAsStringAsync());

            var getResult = await client.GetAsync(id.ToString());
            Assert.True(getResult.IsSuccessStatusCode);
            var json = await getResult.Content.ReadAsStringAsync();

            Assert.Contains(firstName, json);
            Assert.Contains(lastName, json);
        }

        [Theory, AutoData]
        public async Task when_update_customer_then_changes_are_persisted(string firstName, string lastName, string newFirstName, string newLastName)
        {
            var createResult = await client.PostAsync($"?firstName={Uri.EscapeDataString(firstName)}&lastName={Uri.EscapeDataString(lastName)}", null);
            var id = JsonSerializer.Deserialize<Guid>(await createResult.Content.ReadAsStringAsync());

            var updateResult = await client.PutAsync($"{id}/?firstName={Uri.EscapeDataString(newFirstName)}&lastName={Uri.EscapeDataString(newLastName)}", null);
            Assert.True(updateResult.IsSuccessStatusCode);

            var getResult = await client.GetAsync(id.ToString());
            var json = await getResult.Content.ReadAsStringAsync();

            Assert.Contains(newFirstName, json);
            Assert.Contains(newLastName, json);
        }

        [Theory, AutoData]
        public async Task when_update_unknown_customer_then_bad_request(Guid id, string firstName, string lastName)
        {
            var updateResult = await client.PutAsync($"{id}/?firstName={Uri.EscapeDataString(firstName)}&lastName={Uri.EscapeDataString(lastName)}", null);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, updateResult.StatusCode);
        }
    }
}
