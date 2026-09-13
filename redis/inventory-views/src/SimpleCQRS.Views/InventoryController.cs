using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RedisEvents.EventSourcing;

namespace SimpleCQRS.Views
{
    [ApiController]
    public class InventoryController : ControllerBase
    {
        readonly ILogger<InventoryController> logger;
        readonly IViewStore<InventoryItemListDto> inventoryListView;
        readonly IViewStore<InventoryItemDetailsDto> inventoryDetailView;

        public InventoryController(ILogger<InventoryController> logger, IViewStore<InventoryItemListDto> inventoryListView, IViewStore<InventoryItemDetailsDto> inventoryDetailView)
        {
            this.logger = logger;
            this.inventoryDetailView = inventoryDetailView;
            this.inventoryListView = inventoryListView;
        }

        [HttpGet("/items/")]
        [ProducesResponseType(typeof(InventoryItemListDto[]), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> ItemList()
        {
            var items = new List<InventoryItemListDto>();
            await foreach (var item in inventoryListView.ListAsync())
                items.Add(item);

            return Ok(items);
        }

        [HttpGet("/items/{id}")]
        [ProducesResponseType(typeof(InventoryItemDetailsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Item(Guid id)
        {
            var item = await inventoryDetailView.GetAsync(id.ToString());
            if (item != null)
                return Ok(item);

            logger.LogDebug($"received request for unknown id {id}");
            return NotFound();
        }
    }
}
