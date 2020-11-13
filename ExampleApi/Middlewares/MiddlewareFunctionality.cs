using ExampleApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleApi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class MiddlewareFunctionality
    {
        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly IMemoryCache _cache;
        private const string cacheKey = "_values";
        private const string cacheKeyWithoutExpiration = "_valuesWithoutExpiration";
        private const string availability = "_avalaibility";

        public MiddlewareFunctionality(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _httpClient = new HttpClient();
            _semaphoreSlim = new SemaphoreSlim(1);
            _cache = cache;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var path = httpContext.Request.Path;
            var channel = httpContext.Request.Headers["Channel"];
            var method = httpContext.Request.Method;


            if (method == "GET" && !string.IsNullOrWhiteSpace(channel) && httpContext.Request.Path.Equals("/api/functionality/1", StringComparison.Ordinal))
            {
                await _semaphoreSlim.WaitAsync();
                try
                {
                    if (!_cache.TryGetValue(cacheKey, out FunctionalityDetail value))
                    {
                        //_httpClient.DefaultRequestHeaders.Authorization = 
                        //    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                        //    System.Text.Encoding.ASCII.GetBytes($"admin:admin123")));
                        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await _httpClient.GetAsync($"https://localhost:6001{path}");
                        response.EnsureSuccessStatusCode();
                        var responseBody = await response.Content.ReadAsStringAsync();
                        value = JsonSerializer.Deserialize<FunctionalityDetail>(responseBody, new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = true
                        });

                        httpContext.Items.Add(availability, value.Availability.BusinessHours.Includes);

                        _cache.Set(cacheKey, value, new MemoryCacheEntryOptions() // Opciones de cache.
                        {
                            // Podemos establecer la caducidad real de la entrada de caché, siempre (AbsoluteExpiration > SlidingExpiration)
                            AbsoluteExpiration = DateTime.Now.AddMinutes(10),
                            // Establece la prioridad de mantener la entrada de la caché en la caché
                            Priority = CacheItemPriority.Normal,
                            // Permite establecer el tamaño de esta caché en particular, para que no comience a consumir los recursos del servidor.
                            Size = 1024,
                            // Mantener en caché durante este tiempo, restablecer el tiempo si se accede.
                            SlidingExpiration = TimeSpan.FromMinutes(2)
                        });

                        _cache.Set(cacheKeyWithoutExpiration, value, new MemoryCacheEntryOptions()
                        {
                            Priority = CacheItemPriority.Normal,
                            Size = 1024,
                            SlidingExpiration = TimeSpan.FromMinutes(10)
                        });
                    }

                    Console.WriteLine("Ok");
                }
                catch (Exception e)
                {
                    if (_cache.TryGetValue(cacheKeyWithoutExpiration, out FunctionalityDetail values))
                    {
                        Console.WriteLine("Ok");
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }

            await _next.Invoke(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareFunctionalityExtensions
    {
        public static IApplicationBuilder UseMiddlewareFunctionality(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MiddlewareFunctionality>();
        }
    }
}
