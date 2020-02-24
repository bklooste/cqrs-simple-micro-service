

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System;

namespace SimpleCQRS.API
{
    public class Startup
    {
        private ConnectionMultiplexer redisMultiplexer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var connectionString = Configuration.GetConnectionString("RedisConnection");
            this.redisMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            services.AddTransient<IDatabase>(svc => redisMultiplexer.GetDatabase()) ;

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifeTime)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            redisMultiplexer.ConnectionFailed += (sender, args) => appLifeTime.StopApplication();

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
