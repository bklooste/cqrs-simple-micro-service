﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using EventStore.ClientAPI;

namespace SimpleCQRS.API
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<ActionResult> Add(string name)
        {
            var bl = new InventoryItemLogic(Guid.NewGuid(), name);
            await connection.Save(bl, ExpectedVersion.NoStream);
            return NoContent();
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

        //[HttpPost]
        //public async Task<ActionResult> ChangeNameWithoutVersion(Guid id, string name)
        //{
        //    var item = await connection.GetById<InventoryItem>(id);

        //    lock (item) //all uses must use lock if clients dont manage concurrency
        //    {
        //        item.ChangeName(name);
        //        Task.Run(async () => await connection.Save(item, ExpectedVersion.NoStream)).Wait(); // do we lock the object till its persisted ?
        //    }
        //    return NoContent();
        //}

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
    }
}
