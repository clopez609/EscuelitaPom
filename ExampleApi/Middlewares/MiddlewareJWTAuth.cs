using ExampleApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class MiddlewareJWTAuth
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private static readonly string secretKey = "SecretKeywqewqeqqqqqqqqqqqweeeeeeeeeeeeeeeeeeeqweqe";

        public MiddlewareJWTAuth(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            //// Si la ruta de la solicitud coincide, genere token por middleware
            //if (httpContext.Request.Path.Equals(TokenProviderOption.Path, StringComparison.Ordinal))
            //{
            //    await GenerateToken(httpContext);
            //}

            if (!TryRetrieveToken(httpContext, out string token))
            {
                //httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                throw new Exception("No autorizado");
            }

            await _next.Invoke(httpContext);
        }

        private bool TryRetrieveToken(HttpContext httpContext, out string token)
        {
            token = null;
            StringValues authzHeaders;

            if (!httpContext.Request.Headers.TryGetValue("Authorization", out authzHeaders) || authzHeaders.Count() > 1)
            {
                return false;
            }

            var bearerToken = authzHeaders.ElementAt(0);
            token = bearerToken.StartsWith("Bearer ") ?
                    bearerToken.Substring(7) : bearerToken;

            var handler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                // Validar la firma del emisor
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

                // Validar el emisor JWT (iss)
                ValidateIssuer = false,
                ValidIssuer = "https://localhost:7001/",

                // Validar la audiencia JWT (aud)
                ValidateAudience = false,
                //ValidAudience = "https://localhost:44349/",

                // Validar la caducidad del token
                ValidateLifetime = true,
            };

            try
            {
                SecurityToken validatedToken;
                handler.ValidateToken(token, tokenValidationParameters, out validatedToken);
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }

            return true;
        }

        private async Task GenerateToken(HttpContext httpContext)
        {
            var username = "admin";
            var password = "admin123";

            var identity = GetIdentity(username, password);
            if (identity == null)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Usuario o Contraseña invalido.");
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
                    _configuration["Issuer"],
                    _configuration["Audience"],
                    identity,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddMinutes(5)
                );

                //Token
                var jwt = new JwtSecurityToken(header, payload);

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new TokenResponse()
                {
                    AccessToken = encodedJwt,
                    ExpiresIn = (int)TimeSpan.FromMinutes(5).TotalSeconds
                }));

                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private Claim[] GetIdentity(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                return new Claim[] {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, "Admin"),
                };
            }
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
