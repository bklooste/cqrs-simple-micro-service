using System.Linq;

using Microsoft.Extensions.Logging;

using Xunit;
using AutoFixture.Xunit2;

using SimpleCQRS.Views;

namespace SimpleCQRS.API.Test
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]
    public class EventProjectorTests
    {
        readonly ILogger<EventProjector> logger = new LoggerFactory().CreateLogger<EventProjector>();

        [Theory, AutoData]
        public void when_project_then_all_views_are_projected_to(InventoryItemCreated msg)
        {
            var listView = new InventoryListView();
            var detailView = new InventoryItemDetailView();
            var projector = new EventProjector(listView, detailView, logger);

            projector.Project(msg);

            Assert.Single(listView.Repository);
            Assert.Single(detailView.Repository);
        }
    }
}

