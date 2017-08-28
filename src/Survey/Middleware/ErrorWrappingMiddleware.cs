using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Wikiled.Survey.Data;

namespace Wikiled.Survey.Middleware
{
    public class ErrorWrappingMiddleware
    {
        private readonly RequestDelegate next;

        private readonly ILogger<ErrorWrappingMiddleware> logger;

        public ErrorWrappingMiddleware(RequestDelegate next, ILogger<ErrorWrappingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next.Invoke(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(new EventId(999, "GlobalException"), ex, ex.Message);
                context.Response.StatusCode = 500;
            }

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = new ApiResponse(context.Response.StatusCode);
                var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings
                                                                 {
                                                                     ContractResolver = new CamelCasePropertyNamesContractResolver()
                                                                 });

                await context.Response.WriteAsync(json).ConfigureAwait(false);
            }
        }
    }
}
