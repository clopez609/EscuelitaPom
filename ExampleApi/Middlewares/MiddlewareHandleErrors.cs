using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ExampleApi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class GlobalErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
    };

    public class MiddlewareHandleErrors
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerFactory _loggerFactory;

        public MiddlewareHandleErrors(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _loggerFactory = loggerFactory;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var _logger = _loggerFactory.CreateLogger<MiddlewareHandleErrors>();

            try
            {
                await _next.Invoke(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex.Message}");
                await HandleGlobalExceptionAsync(httpContext);
            }
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

        //private static HttpStatusCode GetErrorCode(Exception e)
        //{
        //    switch (e)
        //    {
        //        case ValidationException _:
        //            return HttpStatusCode.BadRequest;
        //        case FormatException _:
        //            return HttpStatusCode.BadRequest;
        //        case AuthenticationException _:
        //            return HttpStatusCode.Forbidden;
        //        case NotImplementedException _:
        //            return HttpStatusCode.NotImplemented;
        //        default:
        //            return HttpStatusCode.InternalServerError;
        //    }
        //}
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
