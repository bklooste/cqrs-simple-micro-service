using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using EventStore.ClientAPI;

namespace SimpleCQRS.API
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class InventoryCommandController : ControllerBase
    {

        readonly ILogger<InventoryCommandController> logger;
        readonly IEventStoreConnection connection;

        public InventoryCommandController(ILogger<InventoryCommandController> logger, IEventStoreConnection connection)
        {
            this.logger = logger;
            this.connection = connection;
        }

        [HttpPost]
        public async Task<ActionResult> Add(string name, Guid? id = null)
        {

            try
            {
                if (id == null)
                    id = Guid.NewGuid();
                var bl = new InventoryItemLogic(id.Value, name);
                await connection.Save(bl, ExpectedVersion.NoStream);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> ChangeName(Guid id, string name, int version)
        {
            var inventoryItem = await connection.GetById<InventoryItemLogic>(id);
            if (inventoryItem == null)
                return NotFound();

            try
            {
                inventoryItem.ChangeName(name);
                await connection.Save(inventoryItem, version);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Deactivate(Guid id, int version)
        {
            var inventoryItem = await connection.GetById<InventoryItemLogic>(id);
            if (inventoryItem == null)
                return NotFound();

            try
            {
                inventoryItem.Deactivate();
                await connection.Save(inventoryItem, version);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CheckIn(Guid id, int number, int version)
        {
            var inventoryItem = await connection.GetById<InventoryItemLogic>(id);
            if (inventoryItem == null)
                return NotFound();

            try
            {
                inventoryItem.CheckIn(number);
                await connection.Save(inventoryItem, version);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Remove(Guid id, int number, int version)
        {
            var inventoryItem = await connection.GetById<InventoryItemLogic>(id);
            if (inventoryItem == null)
                return NotFound();

            try
            {
                inventoryItem.Remove(number);
                await connection.Save(inventoryItem, version);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public ActionResult IsAvailable()
        {
            return Ok(true);
        }
    }
}
