using System;
using System.Collections.Generic;

namespace SimpleCQRS.Views
{
    public class InventoryItemDetailView
    {
        // So that each projection has its own view/ table. Issues the other way 
        readonly Dictionary<Guid, InventoryItemDetailsDto> repository = new Dictionary<Guid, InventoryItemDetailsDto>();
        public IReadOnlyDictionary<Guid, InventoryItemDetailsDto> Repository { get => this.repository; }

        public InventoryItemDetailView()  {}
        public InventoryItemDetailView(Dictionary<Guid, InventoryItemDetailsDto> repository) 
        {
            this.repository = repository;
        }

        public void Handle(InventoryItemCreated message)
        {
            repository.Add(message.Id, new InventoryItemDetailsDto(message.Id, message.Name, 0,0));
        }

        public void Handle(InventoryItemRenamed message)
        {
            InventoryItemDetailsDto d = GetDetailsItem(message.Id);
            d.Name = message.NewName;
            d.Version = message.Version;
        }

        public void Handle(ItemsRemovedFromInventory message)
        {
            var d = GetDetailsItem(message.Id);
            d.CurrentCount -= message.Count;
            d.Version = message.Version;
        }

        public void Handle(ItemsCheckedInToInventory message)
        {
            var d = GetDetailsItem(message.Id);
            d.CurrentCount += message.Count;
            d.Version = message.Version;
        }

        public void Handle(InventoryItemDeactivated message)
        {
            repository.Remove(message.Id);
        }

        private InventoryItemDetailsDto GetDetailsItem(Guid id)
        {
            if (!repository.TryGetValue(id, out var d))
                throw new InvalidOperationException("did not find the original inventory this shouldnt happen");
            return d;
        }
    }
}
