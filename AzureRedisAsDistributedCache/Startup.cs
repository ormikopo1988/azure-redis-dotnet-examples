using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AzureRedisAsDistributedCache
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // For adding distributed cache capabilities
            services.AddDistributedRedisCache(
                (redisCacheOptions) =>
                {
                    redisCacheOptions.InstanceName = "MyRedisInstanceOnAzure";
                    redisCacheOptions.Configuration = "<your-azure-redis-service-connection-string>"; // connection string
                }
            );

            // For adding session state capabilities
            services.AddSession(
                (options) =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session inside that will be stored in Redis will last for 30 minutes
                }
            );

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // We have to explicitly tell it to use the session state it is not ON by default
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
