using ExampleApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace ExampleApi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class MiddlewareAvailability
    {
        private readonly RequestDelegate _next;
        private const string availability = "_avalaibility";

        public MiddlewareAvailability(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Items.Count == 0)
            {
                await _next.Invoke(httpContext);
            }

            if (httpContext.Items.ContainsKey(availability))
            {
                var values = (IList<WeekdayDetail>)(httpContext.Items[availability]);
                var dayName = (DateTime.Now.DayOfWeek).ToString().Substring(0,3);

                var item = values.Where(x => x.Weekday.StartsWith(dayName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                
                if (item == null) throw new Exception("Error de Disponibilidad");
                
                if (DateTime.Now >= Convert.ToDateTime(item.FromHour) && DateTime.Now <= Convert.ToDateTime(item.ToHour))
                {
                    await _next.Invoke(httpContext);
                }
                else
                {
                    throw new Exception($"Horario de Disponibilidad: {item.FromHour} a {item.ToHour}");
                }
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareAvailabilityExtensions
    {
        public static IApplicationBuilder UseMiddlewareAvailability(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MiddlewareAvailability>();
        }
    }
}
