using System.Text.Json.Serialization;

namespace SimpleCQRS
{
    // The events carry public readonly fields rather than properties, so field-based
    // serialization must be turned on for the source generator to see them.
    [JsonSourceGenerationOptions(IncludeFields = true)]
    [JsonSerializable(typeof(InventoryItemCreated))]
    [JsonSerializable(typeof(InventoryItemRenamed))]
    [JsonSerializable(typeof(ItemsCheckedInToInventory))]
    [JsonSerializable(typeof(ItemsRemovedFromInventory))]
    [JsonSerializable(typeof(InventoryItemDeactivated))]
    public partial class InventoryJsonContext : JsonSerializerContext
    {
    }
}
