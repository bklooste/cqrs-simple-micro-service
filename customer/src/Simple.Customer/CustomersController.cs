using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

using Marten;


namespace Simple.Customers
{
    [Consumes("application/json")]
    [Produces("application/json")]
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
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Create([FromQuery] string firstName, [FromQuery] [Required] string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return BadRequest("customer name was not received");

            using var session = store.LightweightSession();
            var customer = new Customer() { FirstName = firstName, LastName = lastName };
            session.Store(customer);

            await session.SaveChangesAsync();

            return Ok(customer.Id);
        }

        [HttpPut("{id}/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Update([FromRoute] Guid id, string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return BadRequest("customer name was not received");

            using var session = store.LightweightSession();
            var customer = await session.LoadAsync<Customer>(id);
            if (customer == null)
                return BadRequest("no such customer");
            customer.FirstName = firstName;
            customer.LastName = lastName;
            session.Store(customer);

            await session.SaveChangesAsync();
            return Ok();
        }

        // GET: api/customers/5
        [HttpGet("{id}", Name = nameof(Get))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerView), 200)]
        public async Task<ActionResult<string>> Get([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("id was not received");

            using var session = store.LightweightSession();
            return await session.Json.FindByIdAsync<Customer>(id);
        }

        [HttpGet("name={lastNamePrefix}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerView[]), 200)]
        public async Task<ActionResult<string>> GetByName(string lastNamePrefix)
        {
            if (string.IsNullOrEmpty(lastNamePrefix) || lastNamePrefix.Length < 4)
                return BadRequest("name of at least 4 characters was not received");

            using var session = store.LightweightSession();
            var json = await session.QueryAsync(new FindCustomerJsonByNameQuery { LastNamePrefix = lastNamePrefix });

            return Ok(json);
        }

        [HttpGet("IsAvailable")]
        public ActionResult IsAvailable()
        {
            return Ok(true);
        }
    }
}
