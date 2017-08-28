using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using ServiceStack.Redis;
using Wikiled.Survey.Middleware;

namespace Wikiled.Survey
{
    public class Startup
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Needed to add this section, and....
            services.AddCors(
              options =>
              {
                  options.AddPolicy(
              "CorsPolicy",
              itemBuider => itemBuider.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials());
              });

            ConfigureRedis(services);
            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //add NLog to .NET Core
            loggerFactory.AddNLog();
            //Enable ASP.NET Core features (NLog.web) - only needed for ASP.NET Core users
            app.AddNLogWeb();
            env.ConfigureNLog("nlog.config");

            app.UseMiddleware<ErrorWrappingMiddleware>();
            app.Use(
                async (context, next) =>
                {
                    await next().ConfigureAwait(false);
                    if (context.Response.StatusCode == 404 &&
                        !Path.HasExtension(context.Request.Path.Value) &&
                        !context.Request.Path.Value.StartsWith("/api/"))
                    {
                        context.Request.Path = "/index.html";
                        await next().ConfigureAwait(false);
                    }
                });

            app.UseMvcWithDefaultRoute();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            logger.Debug("Starting...");
        }

        private void ConfigureRedis(IServiceCollection services)
        {
            var redisConnection = Configuration.GetConnectionString("RedisConnection");
            var manager = new RedisManagerPool(redisConnection);
            services.AddSingleton<IRedisClientsManager>(manager);
        }
    }
}
