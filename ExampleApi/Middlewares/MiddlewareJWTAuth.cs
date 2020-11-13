using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ExampleApi.Middlewares
{
    public class TokenProviderOptions
    {
        public string Path { get; set; } = "/api/token";
        public string Issuer { get; set; } = "https://localhost:44349/";
        public string Audience { get; set; } = "https://localhost:44349/";
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);
    }

    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class MiddlewareJWTAuth
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;
        private static readonly string secretKey = "SecretKeywqewqeqqqqqqqqqqqweeeeeeeeeeeeeeeeeeeqweqe";
        public MiddlewareJWTAuth(RequestDelegate next, IOptions<TokenProviderOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Si la ruta de la solicitud no coincide, omita
            if (!httpContext.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                await _next.Invoke(httpContext);
            }

            // La solicitud debe ser POST con Content-Type: application/x-www-form-urlencoded
            if (httpContext.Request.Method.Equals("POST"))
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Bad request.");
            }

            await GenerateToken(httpContext);
        }

        //Recupera el token de la petición
        // Validacion
        //if (!TryRetrieveToken(httpContext, out string token))
        //{
        //    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        //    return Invoke(httpContext);
        //
        //private bool TryRetrieveToken(HttpContext httpContext, out string token)
        //{
        //    token = null;
        //    StringValues authzHeaders;
        //    if (!httpContext.Request.Headers.TryGetValue("Authorization", out authzHeaders) || authzHeaders.Count() > 1)
        //    {
        //        return false;
        //    }
        //    var bearerToken = authzHeaders.ElementAt(0);
        //    token = bearerToken.StartsWith("Bearer ") ?
        //            bearerToken.Substring(7) : bearerToken;
        //    return true;
        //}

        private async Task GenerateToken(HttpContext httpContext)
        {
            var username = "admin";
            var password = "admin123";

            var identity = GetIdentity(username, password);
            if (identity == null)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Usuario o contraseña invalido.");
                return;
            }

            try
            {
                //Header 
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
                var header = new JwtHeader(signingCredentials);

                //Paylod
                var payload = new JwtPayload(
                    _options.Issuer,
                    _options.Audience,
                    identity,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddMinutes(5)
                );

                //Token
                var jwt = new JwtSecurityToken(header, payload);

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                var response = new
                {
                    access_token = encodedJwt,
                    expires_in = (int)_options.Expiration.TotalSeconds
                };

                // Serialize and return the response
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private Claim[] GetIdentity(string username, string password)
        {
            // DON'T do this in production, obviously!
            if (username == "admin" && password == "admin123")
            {
                return new Claim[] {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, "Admin"),
                };
            }

            // Credentials are invalid, or account doesn't exist
            return null;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareJWTAuthExtensions
    {
        public static IApplicationBuilder UseMiddlewareJWTAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MiddlewareJWTAuth>();
        }
    }
}
