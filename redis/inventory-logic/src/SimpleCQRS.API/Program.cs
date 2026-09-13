using Microsoft.OpenApi;

using RedisEvents.EventSourcing;

using SimpleCQRS;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventStore("inventory", InventoryEventTypes.Register);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseRouting();

app.UseSwagger();

app.UseSwaggerUI(c =>      //Swagger UI should be served from static container not service
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

//app.UseAuthorization();            // maybe done by gateway

app.MapControllers();

app.Run();

/// <summary>The entry point type, exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can host this service in tests.</summary>
public partial class Program;
