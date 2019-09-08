
using System;
using System.Collections.Generic;

namespace SimpleCQRS.API
{
    public abstract class AggregateRoot
    {
        readonly List<Event> changes = new List<Event>();
        public abstract Guid Id { get; }
        public int Version { get; internal set; }

        public IEnumerable<Event> GetUncommittedChanges()
        {
            return changes;
        }

        public void MarkChangesAsCommitted()
        {
            changes.Clear();
        }

        public void LoadsFromHistory(IEnumerable<Event> history)
        {
            foreach (var e in history)
                ApplyChange(e, false);
        }

        protected void ApplyChange(Event @event)
        {
            ApplyChange(@event, true);
        }

        // push atomic aggregate changes to local history for further processing (EventStore.SaveEvents)
        private void ApplyChange(Event @event, bool isNew)
        {
            dynamic s = this;
            s.Apply((dynamic) @event);
            if(isNew)
                changes.Add(@event);
        }
    }

}
