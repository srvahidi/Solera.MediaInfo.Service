using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Solera.MediaInfo.Service.Constants;
using Solera.MediaInfo.Service.Models;
using System;
using System.Threading.Tasks;

namespace Solera.MediaInfo.Service.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly string _product, _layer;

        public ExceptionMiddleware(string product, string layer, RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _product = product;
            _layer = layer;
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return context.Response.WriteAsync(JsonConvert.SerializeObject(
                new Response<string>(false, StatusCodes.Status500InternalServerError,
                null, exception.Message), JsonSettings.CamelCase));
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app, string product, string layer)
        {
            app.UseMiddleware<ExceptionMiddleware>(product, layer);
        }
    }
}
