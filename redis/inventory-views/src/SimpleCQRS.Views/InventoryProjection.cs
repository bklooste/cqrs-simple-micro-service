using System.Threading;
using System.Threading.Tasks;

using RedisEvents.EventSourcing;

using SimpleCQRS;

namespace SimpleCQRS.Views
{
    // Keeps both read models up to date from inventory events. RedisEvents delivers at least once,
    // so this projection can be asked to handle the same event twice. The list view is a plain
    // SetAsync/DeleteAsync per id, which is naturally idempotent. The detail view carries a running
    // count, so every accumulative handler reads the current view first and skips the update when
    // meta.Version is not newer than what's already stored - without that guard a redelivered
    // ItemsCheckedInToInventory/ItemsRemovedFromInventory would apply its count a second time.
    public class InventoryProjection :
        IProjection<InventoryItemCreated>,
        IProjection<InventoryItemRenamed>,
        IProjection<ItemsCheckedInToInventory>,
        IProjection<ItemsRemovedFromInventory>,
        IProjection<InventoryItemDeactivated>
    {
        readonly IViewStore<InventoryItemListDto> listView;
        readonly IViewStore<InventoryItemDetailsDto> detailView;

        public InventoryProjection(IViewStore<InventoryItemListDto> listView, IViewStore<InventoryItemDetailsDto> detailView)
        {
            this.listView = listView;
            this.detailView = detailView;
        }

        public async ValueTask HandleAsync(InventoryItemCreated @event, EventMeta meta, CancellationToken ct)
        {
            await listView.SetAsync(@event.Id.ToString(), new InventoryItemListDto(@event.Id, @event.Name), ct);
            await detailView.SetAsync(@event.Id.ToString(), new InventoryItemDetailsDto(@event.Id, @event.Name, 0, meta.Version), ct);
        }

        public async ValueTask HandleAsync(InventoryItemRenamed @event, EventMeta meta, CancellationToken ct)
        {
            var list = await listView.GetAsync(@event.Id.ToString(), ct);
            if (list != null)
            {
                list.Name = @event.NewName;
                await listView.SetAsync(@event.Id.ToString(), list, ct);
            }

            var detail = await detailView.GetAsync(@event.Id.ToString(), ct);
            if (detail is null || detail.Version >= meta.Version)
                return;

            detail.Name = @event.NewName;
            detail.Version = meta.Version;
            await detailView.SetAsync(@event.Id.ToString(), detail, ct);
        }

        public async ValueTask HandleAsync(ItemsCheckedInToInventory @event, EventMeta meta, CancellationToken ct)
        {
            var detail = await detailView.GetAsync(@event.Id.ToString(), ct);
            if (detail is null || detail.Version >= meta.Version)
                return;

            detail.CurrentCount += @event.Count;
            detail.Version = meta.Version;
            await detailView.SetAsync(@event.Id.ToString(), detail, ct);
        }

        public async ValueTask HandleAsync(ItemsRemovedFromInventory @event, EventMeta meta, CancellationToken ct)
        {
            var detail = await detailView.GetAsync(@event.Id.ToString(), ct);
            if (detail is null || detail.Version >= meta.Version)
                return;

            detail.CurrentCount -= @event.Count;
            detail.Version = meta.Version;
            await detailView.SetAsync(@event.Id.ToString(), detail, ct);
        }

        public async ValueTask HandleAsync(InventoryItemDeactivated @event, EventMeta meta, CancellationToken ct)
        {
            await listView.DeleteAsync(@event.Id.ToString(), ct);
            await detailView.DeleteAsync(@event.Id.ToString(), ct);
        }
    }
}
