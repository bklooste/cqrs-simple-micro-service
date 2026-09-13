using RedisEvents.EventSourcing;

using Scalar.AspNetCore;

using SimpleCQRS;
using SimpleCQRS.API;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventStore("inventory", InventoryEventTypes.Register);

builder.Services.AddSingleton<ExternalLogic>();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.MapOpenApi();
app.MapScalarApiReference();

var group = app.MapGroup("/InventoryCommand");

group.MapPost("/Add", async (IEventRepository repository, ILogger<Program> logger, string name, Guid? id) =>
{
    try
    {
        var itemId = id ?? Guid.NewGuid();
        var bl = new InventoryItemLogic(itemId, name);
        await repository.SaveAsync(bl, 0);
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (ConcurrencyException)
    {
        return Results.Conflict();
    }
});

group.MapPost("/ChangeName", async (IEventRepository repository, Guid id, string name, int version) =>
{
    try
    {
        var inventoryItem = await repository.LoadAsync<InventoryItemLogic>(id.ToString());
        if (inventoryItem == null)
            return Results.NotFound();

        inventoryItem.ChangeName(name);
        await repository.SaveAsync(inventoryItem, version);
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (ConcurrencyException)
    {
        return Results.Conflict();
    }
});

group.MapPost("/Deactivate", async (IEventRepository repository, Guid id, int version) =>
{
    try
    {
        var inventoryItem = await repository.LoadAsync<InventoryItemLogic>(id.ToString());
        if (inventoryItem == null)
            return Results.NotFound();

        inventoryItem.Deactivate();
        await repository.SaveAsync(inventoryItem, version);
        return Results.NoContent();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (ConcurrencyException)
    {
        return Results.Conflict();
    }
});

group.MapPost("/CheckIn", async (IEventRepository repository, ExternalLogic logic, Guid id, int number, int version) =>
{
    //Test its there in integration !
    var price = logic.GetPrice();
    try
    {
        var inventoryItem = await repository.LoadAsync<InventoryItemLogic>(id.ToString());
        if (inventoryItem == null)
            return Results.NotFound();

        inventoryItem.CheckIn(number, price);
        await repository.SaveAsync(inventoryItem, version);
        return Results.NoContent();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (ConcurrencyException)
    {
        return Results.Conflict();
    }
});

group.MapPost("/Remove", async (IEventRepository repository, Guid id, int number, int version) =>
{
    try
    {
        var inventoryItem = await repository.LoadAsync<InventoryItemLogic>(id.ToString());
        if (inventoryItem == null)
            return Results.NotFound();

        inventoryItem.Remove(number);
        await repository.SaveAsync(inventoryItem, version);
        return Results.NoContent();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (ConcurrencyException)
    {
        return Results.Conflict();
    }
});

group.MapGet("/IsAvailable", () => Results.Ok(true));

app.Run();

/// <summary>The entry point type, exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can host this service in tests.</summary>
public partial class Program;
