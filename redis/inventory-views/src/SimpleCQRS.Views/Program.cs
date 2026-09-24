using RedisEvents.EventSourcing;
using RedisEvents.Projections;

using Scalar.AspNetCore;

using SimpleCQRS.Views;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventProjector("inventory", InventoryEventTypes.Register)
       .AddRedisViewStore<InventoryItemDetailsDto>("inventory", "detail", InventoryJsonContext.Default.InventoryItemDetailsDto)
       .AddProjection<InventoryProjection>();

builder.Services.AddSingleton<IViewStore<InventoryItemListDto>, InMemoryViewStore<InventoryItemListDto>>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/items/", async (IViewStore<InventoryItemListDto> inventoryListView) =>
{
    var items = new List<InventoryItemListDto>();
    await foreach (var item in inventoryListView.ListAsync())
        items.Add(item);

    return Results.Ok(items);
});

app.MapGet("/items/{id}", async (IViewStore<InventoryItemDetailsDto> inventoryDetailView, ILogger<Program> logger, Guid id) =>
{
    var item = await inventoryDetailView.GetAsync(id.ToString());
    if (item != null)
        return Results.Ok(item);

    logger.LogDebug($"received request for unknown id {id}");
    return Results.NotFound();
});

app.Run();

/// <summary>The entry point type, exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can host this service in tests.</summary>
public partial class Program;
