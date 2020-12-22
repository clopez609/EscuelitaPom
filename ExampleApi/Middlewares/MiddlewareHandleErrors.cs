using ExampleApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ExampleApi.Middlewares
{
    public class MiddlewareHandleErrors
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MiddlewareHandleErrors> _logger;

        public MiddlewareHandleErrors(RequestDelegate next, ILogger<MiddlewareHandleErrors> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                _logger.LogInformation("Begin Errors Handler Middleware");
                await _next.Invoke(httpContext);
            }
            catch (SecurityTokenExpiredException)
            {
                throw new SecurityTokenExpiredException();
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                throw new SecurityTokenInvalidIssuerException();
            }
            catch (Exception)
            {
                await HandleGlobalExceptionAsync(httpContext);
            };
        }

        private static Task HandleGlobalExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(JsonConvert.SerializeObject(new GlobalErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Something went wrong !Internal Server Error"
            }));
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareHandleErrorsExtensions
    {
        public static IApplicationBuilder UseMiddlewareHandleErrors(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MiddlewareHandleErrors>();
        }
    }
}
