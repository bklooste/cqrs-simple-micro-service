using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

using Marten;

namespace Simple.Customers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {

        readonly ILogger<CustomersController> logger; 
        readonly IDocumentStore store;

        public CustomersController(ILogger<CustomersController> logger, IDocumentStore store)
        {
            this.logger = logger;
            this.store = store;
            this.logger.LogDebug("InventoryCommandController invoked, Note core already does all request/ request time and failure logging");
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Create(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return BadRequest("customer name was not received");

            using var session = store.LightweightSession();
            var customer = new Customer() { FirstName = firstName, LastName = lastName };
            session.Store(customer);

            await session.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), "Customers", new { id = customer.Id });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Update(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return BadRequest("customer name was not received");

            using var session = store.LightweightSession();
            var customer = new Customer() { FirstName = firstName, LastName = lastName };
            session.Store(customer);

            await session.SaveChangesAsync();
            return Ok( );
        }

        // GET: api/customers/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Customer), 200)]
        public async Task<ActionResult<Customer>> Get(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("id was not received");

            using var session = store.LightweightSession();
            return await session.LoadAsync<Customer>(id);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Customer), 200)]
        public async Task<ActionResult<Guid>> GetByName(string lastNamePrefix)
        {
            if (string.IsNullOrEmpty(lastNamePrefix) || lastNamePrefix.Length < 4 )
                return BadRequest("name of at least 4 characters was not received");

            using var session = store.LightweightSession();
            var result = await session.QueryAsync(new  FindCustomerByNameQuery{ LastNamePrefix = lastNamePrefix });

            return Ok(result);
        }

        [HttpGet]
        public ActionResult IsAvailable()
        {
            return Ok(true);
        }
    }
}
