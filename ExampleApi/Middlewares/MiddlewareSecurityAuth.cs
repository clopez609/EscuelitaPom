using ExampleApi.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExampleApi.Middlewares
{
    public class MiddlewareSecurityAuth
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public MiddlewareSecurityAuth(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            StringValues Headers;

            if (!httpContext.Request.Headers.TryGetValue("Authorization", out Headers) || Headers.Count() > 1)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                throw new NotImplementedException("Authorization Key Empty");
            }

            var authHeader = Headers.ElementAt(0);

            if (authHeader.StartsWith(Constants.Auth.Basic) && authHeader != null)
            {
                var encodedCredentials = authHeader.Substring(6);

                if (!string.IsNullOrWhiteSpace(encodedCredentials))
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
            }
            else if (authHeader.StartsWith(Constants.Auth.Jwt) && authHeader != null)
            {
                var token = authHeader.Substring(7);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        // Validar la firma del emisor
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SECRET_KEY"])),

                        // Validar el emisor JWT (iss)
                        ValidateIssuer = true,
                        ValidIssuer = _configuration["ISSUER"],

                        // Validar la audiencia JWT (aud)
                        ValidateAudience = false,
                        ValidAudience = _configuration["AUDIENCE"],

                        // Validar la caducidad del token
                        ValidateLifetime = true,
                    };
                    try
                    {
                        SecurityToken validatedToken;
                        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                        handler.ValidateToken(token, tokenValidationParameters, out validatedToken);
                    }
                    catch (SecurityTokenExpiredException)
                    {
                        throw new SecurityTokenExpiredException();
                    }
                    catch (SecurityTokenInvalidIssuerException)
                    {
                        throw new SecurityTokenInvalidIssuerException();
                    };

                    await _next.Invoke(httpContext);
                    return;
                }
            }

            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            throw new NotImplementedException("Request Headers Invalid");
        }

        public bool IsAuthorized(string username, string password)
        {
            // Check that username and password are correct
            return username.Equals(_configuration["USER"], StringComparison.InvariantCultureIgnoreCase)
                   && password.Equals(_configuration["PASS"]);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareSecurityAuthExtensions
    {
        public static IApplicationBuilder UseMiddlewareSecurityAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MiddlewareSecurityAuth>();
        }
    }
}
