using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Marten;


namespace Simple.Customers
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

            var connectionString = Configuration.GetConnectionString("PostgresConnection");



            var store = DocumentStore.For(storeOptions =>
           {
               storeOptions.Connection(connectionString);
               storeOptions.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
               storeOptions.CreateDatabasesForTenants(c =>                  
                       c.ForTenant()
                           .CheckAgainstPgDatabase()
                           .WithOwner("postgres")
                           .WithEncoding("UTF-8")
                           .ConnectionLimit(-1)                  
                   );
           });

            services.AddSingleton<IDocumentStore>(store);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Customer API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseSwagger();

            app.UseSwaggerUI(c =>      //Swagger UI should be served from static container not service
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            //app.UseAuthorization();            // maybe done by gateway 

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
