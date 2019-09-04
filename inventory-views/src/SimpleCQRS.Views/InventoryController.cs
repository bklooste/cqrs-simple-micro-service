using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SimpleCQRS.Views
{
    [ApiController]
    public class InventoryController : ControllerBase
    {
        readonly ILogger<InventoryController> logger;
        readonly IReadOnlyList<InventoryItemListDto> inventoryListRepository; 
        readonly IReadOnlyDictionary<Guid, InventoryItemDetailsDto> inventoryDetailRepository;

        public InventoryController(ILogger<InventoryController> logger, IReadOnlyList<InventoryItemListDto> inventoryListRepository, IReadOnlyDictionary<Guid, InventoryItemDetailsDto> inventoryDetailRepository)
        {
            this.logger = logger;
            this.inventoryDetailRepository = inventoryDetailRepository;
            this.inventoryListRepository = inventoryListRepository;
        }

        [HttpGet("/items/")]
        [ProducesResponseType(typeof(InventoryItemListDto[]), 200)]
        [ProducesResponseType(500)]
        public ActionResult ItemList()
        {          
            return Ok(inventoryListRepository);
        }

        [HttpGet("/items/{id}")] 
        [ProducesResponseType(typeof(InventoryItemDetailsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult Item(Guid id) 
        {
            if (inventoryDetailRepository.TryGetValue(id, out var item))
                return Ok(item);

            logger.LogDebug($"received request for unknown id {item}");
            return NotFound();
        }
    }
}
