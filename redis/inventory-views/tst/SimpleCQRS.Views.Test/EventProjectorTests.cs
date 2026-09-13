using System.Threading;
using System.Threading.Tasks;

using AutoFixture.Xunit2;

using RedisEvents.EventSourcing;

using SimpleCQRS;
using SimpleCQRS.Views;

using Xunit;

namespace SimpleCQRS.API.Test
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]
    public class EventProjectorTests
    {
        static EventMeta Meta(string id, int version) => new(id, version, RedisEvents.Wire.StreamId.Min, string.Empty);

        [Theory, AutoData]
        public async Task when_created_then_both_views_are_projected_to(InventoryItemCreated msg)
        {
            var listView = new InMemoryViewStore<InventoryItemListDto>();
            var detailView = new InMemoryViewStore<InventoryItemDetailsDto>();
            var projection = new InventoryProjection(listView, detailView);

            await projection.HandleAsync(msg, Meta(msg.Id.ToString(), 1), CancellationToken.None);

            var listItem = await listView.GetAsync(msg.Id.ToString());
            var detailItem = await detailView.GetAsync(msg.Id.ToString());

            Assert.NotNull(listItem);
            Assert.NotNull(detailItem);
            Assert.Equal(msg.Name, listItem!.Name);
            Assert.Equal(msg.Name, detailItem!.Name);
            Assert.Equal(0, detailItem.CurrentCount);
        }

        [Theory, AutoData]
        public async Task given_existing_item_when_receive_items_checked_in_then_expected_count_correct(ItemsCheckedInToInventory msg, int preCount, string name)
        {
            var listView = new InMemoryViewStore<InventoryItemListDto>();
            var detailView = new InMemoryViewStore<InventoryItemDetailsDto>();
            await detailView.SetAsync(msg.Id.ToString(), new InventoryItemDetailsDto(msg.Id, name, preCount, 1));
            var projection = new InventoryProjection(listView, detailView);

            await projection.HandleAsync(msg, Meta(msg.Id.ToString(), 2), CancellationToken.None);

            var detailItem = await detailView.GetAsync(msg.Id.ToString());
            Assert.Equal(preCount + msg.Count, detailItem!.CurrentCount);
        }

        [Theory, AutoData]
        public async Task given_stale_redelivery_when_receive_items_checked_in_then_count_is_not_applied_twice(ItemsCheckedInToInventory msg, int preCount, string name)
        {
            var listView = new InMemoryViewStore<InventoryItemListDto>();
            var detailView = new InMemoryViewStore<InventoryItemDetailsDto>();
            // Version 2 has already been recorded - a redelivery of the same event at version 2 must be ignored.
            await detailView.SetAsync(msg.Id.ToString(), new InventoryItemDetailsDto(msg.Id, name, preCount, 2));
            var projection = new InventoryProjection(listView, detailView);

            await projection.HandleAsync(msg, Meta(msg.Id.ToString(), 2), CancellationToken.None);

            var detailItem = await detailView.GetAsync(msg.Id.ToString());
            Assert.Equal(preCount, detailItem!.CurrentCount);
        }

        [Theory, AutoData]
        public async Task when_deactivated_then_both_views_remove_the_item(InventoryItemDeactivated msg, string name)
        {
            var listView = new InMemoryViewStore<InventoryItemListDto>();
            var detailView = new InMemoryViewStore<InventoryItemDetailsDto>();
            await listView.SetAsync(msg.Id.ToString(), new InventoryItemListDto(msg.Id, name));
            await detailView.SetAsync(msg.Id.ToString(), new InventoryItemDetailsDto(msg.Id, name, 0, 1));
            var projection = new InventoryProjection(listView, detailView);

            await projection.HandleAsync(msg, Meta(msg.Id.ToString(), 2), CancellationToken.None);

            Assert.Null(await listView.GetAsync(msg.Id.ToString()));
            Assert.Null(await detailView.GetAsync(msg.Id.ToString()));
        }
    }
}
