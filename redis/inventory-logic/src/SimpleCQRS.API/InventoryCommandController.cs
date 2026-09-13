using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RedisEvents.EventSourcing;

namespace SimpleCQRS.API
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class InventoryCommandController : ControllerBase
    {

        readonly ILogger<InventoryCommandController> logger;
        readonly IEventRepository repository;
        readonly ExternalLogic logic;

        public InventoryCommandController(ILogger<InventoryCommandController> logger, IEventRepository repository)
        {
            this.logger = logger;
            this.repository = repository;
            this.logic = new ExternalLogic();
            this.logger.LogDebug("InventoryCommandController invoked, Note core already does all request/ request time and failure logging");
        }

        [HttpPost]
        public async Task<ActionResult> Add(string name, Guid? id = null)
        {
            try
            {
                if (id == null)
                    id = Guid.NewGuid();
                var bl = new InventoryItemLogic(id.Value, name);
                await repository.SaveAsync(bl, 0);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ConcurrencyException)
            {
                return Conflict();
            }
        }

        [HttpPost]
        public async Task<ActionResult> ChangeName(Guid id, string name, int version)
        {
            try
            {
                var inventoryItem = await repository.LoadAsync<InventoryItemLogic>(id.ToString());
                if (inventoryItem == null)
                    return NotFound();

                inventoryItem.ChangeName(name);
                await repository.SaveAsync(inventoryItem, version);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ConcurrencyException)
            {
                return Conflict();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Deactivate(Guid id, int version)
        {
            try
            {
                var inventoryItem = await repository.LoadAsync<InventoryItemLogic>(id.ToString());
                if (inventoryItem == null)
                    return NotFound();

                inventoryItem.Deactivate();
                await repository.SaveAsync(inventoryItem, version);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ConcurrencyException)
            {
                return Conflict();
            }
        }

        [HttpPost]
        public async Task<ActionResult> CheckIn(Guid id, int number, int version)
        {
            //Test its there in integration !
            var price = this.logic.GetPrice();
            try
            {
                var inventoryItem = await repository.LoadAsync<InventoryItemLogic>(id.ToString());
                if (inventoryItem == null)
                    return NotFound();

                inventoryItem.CheckIn(number, price);
                await repository.SaveAsync(inventoryItem, version);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ConcurrencyException)
            {
                return Conflict();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Remove(Guid id, int number, int version)
        {
            try
            {
                var inventoryItem = await repository.LoadAsync<InventoryItemLogic>(id.ToString());
                if (inventoryItem == null)
                    return NotFound();

                inventoryItem.Remove(number);
                await repository.SaveAsync(inventoryItem, version);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ConcurrencyException)
            {
                return Conflict();
            }
        }

        [HttpGet]
        public ActionResult IsAvailable()
        {
            return Ok(true);
        }
    }
}
