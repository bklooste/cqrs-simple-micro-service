using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCQRS.Views
{
    public class InventoryListView 
    {
        // So that each projection has its own view , it owns it . Issues the other way 
        readonly List<InventoryItemListDto> repository = new List<InventoryItemListDto>();

        public IReadOnlyList<InventoryItemListDto> Repository { get => this.repository; }

        public void Handle(InventoryItemCreated message)
        {
            repository.Add(new InventoryItemListDto(message.Id, message.Name));
        }

        public void Handle(InventoryItemRenamed message)
        {
            var item = repository.First(x => x.Id == message.Id);
            item.Name = message.NewName;
        }

        public void Handle(InventoryItemDeactivated message)
        {
            repository.RemoveAll(x => x.Id == message.Id);
        }

    }
}
