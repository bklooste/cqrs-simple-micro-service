using Microsoft.OpenApi;

using RedisEvents.EventSourcing;

using SimpleCQRS.Views;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventProjector("inventory", InventoryEventTypes.Register)
       .AddRedisViewStore<InventoryItemDetailsDto>("inventory", "detail", InventoryJsonContext.Default.InventoryItemDetailsDto)
       .AddProjection<InventoryProjection>();

builder.Services.AddSingleton<IViewStore<InventoryItemListDto>, InMemoryViewStore<InventoryItemListDto>>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

app.UseRouting();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c =>      //Swagger UI should be served from static container not service
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"Inventory View Service {app.Environment.EnvironmentName}");
});

app.Run();

/// <summary>The entry point type, exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can host this service in tests.</summary>
public partial class Program;
