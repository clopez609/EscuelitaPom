using ExampleApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace ExampleApi
{
    public class Startup
    {
        private static readonly string secretKey = "SecretKeywqewqeqqqqqqqqqqqweeeeeeeeeeeeeeeeeeeqweqe";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                // Validar la firma del emisor
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validar el emisor JWT (iss)
                ValidateIssuer = true,
                ValidIssuer = "https://localhost:7001/",

                // Validar la audiencia JWT (aud)
                ValidateAudience = false,
                //ValidAudience = "https://localhost:44349/",

                // Validar la caducidad del token
                ValidateLifetime = true,
            };

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
            });

            services.AddControllers();

            services.AddMemoryCache();

            services.AddSwaggerGen(opt =>
            {
                var groupName = "v1";

                opt.SwaggerDoc(groupName, new OpenApiInfo
                {
                    Title = $"Example Api {groupName}",
                    Version = groupName,
                    Description = "Example API",
                    Contact = new OpenApiContact
                    {
                        Name = "Example Api Company",
                        Email = string.Empty,
                        Url = new Uri("https://localhost:5001"),
                    }
                });
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Bearer",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description = "Por favor, inserte JTW",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new string[] { }
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region Middlewares

            app.UseMiddlewareHandleErrors();

            app.UseMiddlewareSecurityAuth();

            app.UseMiddlewareFunctionality();

            app.UseMiddlewareAvailability();

            #endregion

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Example Api V1");
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
