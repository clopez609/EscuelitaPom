using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExampleApi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class MiddlewareBasicAuth
    {
        private readonly RequestDelegate _next;
        private readonly string _realm;
        public MiddlewareBasicAuth(RequestDelegate next, string realm)
        {
            _next = next;
            _realm = realm;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string authHeader = httpContext.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                // Get the encoded username and password
                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                // Decode from Base64 to string
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                // Split username and password
                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];
                // Check if login is correct
                if (IsAuthorized(username, password))
                {
                    await _next.Invoke(httpContext);
                    return;
                }
            }

            //WWW-Authenticate: <type> realm = <realm>
            // <type> es el esquema de autenticación
            // El atributo realm se puede establecer a cualquier valor para identificar el área segura

            // Devuelve el tipo de autenticación (hace que el navegador muestre el diálogo de inicio de sesión)
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic";
            // Agregar realm si no es nulo
            if (!string.IsNullOrWhiteSpace(_realm))
            {
                httpContext.Response.Headers["WWW-Authenticate"] += $" realm=\"{_realm}\"";
            }

            // Devuelve unauthorized
            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }

        public bool IsAuthorized(string username, string password)
        {
            // Check that username and password are correct
            return username.Equals("admin", StringComparison.InvariantCultureIgnoreCase)
                   && password.Equals("admin123");
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareBasicAuthExtensions
    {
        public static IApplicationBuilder UseMiddlewareBasicAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MiddlewareBasicAuth>("Mi Servidor");
        }
    }
}
