using System.Text.Json.Serialization;

using SimpleCQRS;

namespace SimpleCQRS.Views
{
    // The events (and the detail view) carry public fields rather than properties, so field-based
    // serialization must be turned on for the source generator to see them.
    [JsonSourceGenerationOptions(IncludeFields = true)]
    [JsonSerializable(typeof(InventoryItemCreated))]
    [JsonSerializable(typeof(InventoryItemRenamed))]
    [JsonSerializable(typeof(ItemsCheckedInToInventory))]
    [JsonSerializable(typeof(ItemsRemovedFromInventory))]
    [JsonSerializable(typeof(InventoryItemDeactivated))]
    [JsonSerializable(typeof(InventoryItemDetailsDto))]
    public partial class InventoryJsonContext : JsonSerializerContext
    {
    }
}
