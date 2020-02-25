using System.Collections.Generic;
using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SimpleCQRS.Views
{
    public class Startup
    {
        EventSubscriber? subscriber;
        IConnectionMultiplexer? redisMultiplexer = null;


        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var connectionString = Configuration.GetConnectionString("RedisConnection");
            this.redisMultiplexer = ConnectionMultiplexer.Connect(connectionString);
       //     services.AddTransient<IDatabase>(svc => redisMultiplexer.GetDatabase());

            var inventoryListView = new InventoryListView();
            var inventoryView = new InventoryItemDetailView();
            services.AddSingleton<IReadOnlyList<InventoryItemListDto>> (inventoryListView.Repository); 
            services.AddSingleton<IReadOnlyDictionary<Guid, InventoryItemDetailsDto>>(inventoryView.Repository);
            services.AddTransient<EventProjector>(svc => new EventProjector(inventoryListView, inventoryView, svc.GetRequiredService<ILogger<EventProjector>>()));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }



        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, EventProjector projector, IHostApplicationLifetime applicationLifeTime)
        {
            //app.UseHttpsRedirection();             //app.UseAuthorization();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>      //Swagger UI should be served from static container not service
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"Inventory View Service {env.EnvironmentName}");
            });

            // if using a DB make the projection / writer a 3rd generic host service.
            redisMultiplexer!.ConnectionFailed += (sender, args) => applicationLifeTime.StopApplication();
            this.subscriber = new EventSubscriber(() => redisMultiplexer!.GetDatabase(), projector.ProjectBatch, applicationLifeTime, app.ApplicationServices.GetRequiredService<ILogger<EventSubscriber>>());
            this.subscriber.Start();
        }
    }
}
