using System;

using RedisEvents.Wire;

namespace SimpleCQRS.Views
{
    public class InventoryItemDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int CurrentCount { get; set; }
        public StreamId StreamPosition { get; set; }

        public InventoryItemDetailsDto(Guid id, string name, int currentCount, StreamId streamPosition)
        {
            Id = id;
            Name = name;
            CurrentCount = currentCount;
            StreamPosition = streamPosition;
        }
    }
}
