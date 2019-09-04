using System.Collections.Generic;
using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;

namespace SimpleCQRS.Views
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IEventStoreConnection>(EventStoreConnection.Create(Configuration.GetConnectionString("EventStoreConnection")));

            var connection = EventStoreConnection.Create(Configuration.GetConnectionString("EventStoreConnection"));
            var inventoryListView = new InventoryListView();
            var inventoryView = new InventoryItemDetailView();

            services.AddSingleton<IReadOnlyList<InventoryItemListDto>> (inventoryListView.Repository); 
            services.AddSingleton<IReadOnlyDictionary<Guid, InventoryItemDetailsDto>>(inventoryView.Repository);

            services.AddSingleton(svc =>  new SubcribeAndProjector(inventoryListView, inventoryView, svc.GetRequiredService<ILogger<SubcribeAndProjector>>()) );




            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }



        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SubcribeAndProjector subcribeAndProjector, IEventStoreConnection connection, IHostApplicationLifetime applicationLifeTime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>      //Swagger UI should be served from static container not service
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            // if using a dB make the projection a 3rd generic host service.
          // subcribeAndProjector.ConfigureAndStart(connection, applicationLifeTime);

        }
    }
}
