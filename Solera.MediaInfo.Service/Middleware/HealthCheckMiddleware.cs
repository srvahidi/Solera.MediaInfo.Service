using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Solera.MediaInfo.Service.Constants;
using Solera.MediaInfo.Service.Models;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Solera.MediaInfo.Service.Middleware
{
    public class HealthCheckMiddleware
    {
        private readonly ILogger _logger;

        public HealthCheckMiddleware(RequestDelegate next, ILogger<HealthCheckMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var assembly = typeof(Startup).Assembly;
            var creationDate = File.GetCreationTime(assembly.Location);
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            var message = $"Media Info Service is Ok, Version: {version}, Last Updated: {creationDate}";
            _logger.LogInformation(message);
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status200OK;

            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(
                new Response<string>(true, StatusCodes.Status200OK, message), JsonSettings.CamelCase));
        }
    }

    public static class HealthCheckMiddlewareExtensions
    {
        public static void UseHealthCheckMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<HealthCheckMiddleware>();
        }
    }
}
