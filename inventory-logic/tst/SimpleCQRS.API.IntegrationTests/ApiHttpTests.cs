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

namespace SimpleCQRS.API.IntegrationTest
{

    [Trait("Integration", "Local")]
    public class ApiHttpTests 
    {
        readonly HttpClient client = new System.Net.Http.HttpClient();

        public ApiHttpTests(EventStoreFixture fixture)
        {
            client.BlockTillAvailable("http://localhost:53107/InventoryCommand/Add?name=rtes" + Guid.NewGuid());
        }

        // if the service does security we can and should test here.

        [Theory, AutoData]
        public async Task when_create_event_then_its_in_store_in_correct_format(Guid id)
        {
            var result = await client.PostAsync($"http://localhost:53107/InventoryCommand/Add?name=&id={id}", null);
            
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}
