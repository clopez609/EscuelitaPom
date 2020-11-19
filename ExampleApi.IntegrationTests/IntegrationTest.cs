using ExampleApi.Helpers;
using ExampleApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ExampleApi.IntegrationTests
{
    public class IntegrationTest
    {
        private readonly TestServer _server;
        protected readonly HttpClient _testClient;

        public IntegrationTest()
        {
            //var appFactory = new WebApplicationFactory<Startup>();
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _testClient = _server.CreateClient();
        }

        protected void AuthenticateBasic()
        {
            _testClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetBasicAsync());
        }

        protected void AuthenticateJwt()
        {
            _testClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetJwtAsync());
        }

        private string GetJwtAsync()
        {
            string secretKey = "SecretKeywqewqeqqqqqqqqqqqweeeeeeeeeeeeeeeeeeeqweqe";

            var identity = new Claim[]
            {
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Role, "Admin"),
            };

            try
            {
                //Header 
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
                var header = new JwtHeader(signingCredentials);

                //Paylod
                var payload = new JwtPayload(
                    "https://localhost:7001/",
                    "https://localhost:5001/",
                    identity,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddMinutes(5)
                );

                //Token
                var jwt = new JwtSecurityToken(header, payload);

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                return encodedJwt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetBasicAsync()
        {
            string username = "admin";
            string password = "admin123";

            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

            return credentials;
        }

        //protected HttpClient ConfigEnviroment(string nameFile)
        //{
        //    //var path = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        //    //var configPath = Path.Combine($"{path}Enviroment", $"{nameFile}");
        //    var appFactory = new WebApplicationFactory<Startup>();
        //        //.WithWebHostBuilder();
        //        //builder =>
        //    //{
        //        //builder.ConfigureAppConfiguration((context, conf) =>
        //        //{
        //        //    conf.AddJsonFile(configPath);
        //        //});
        //    //});
        //    return appFactory.CreateClient();
        //}

    }
}
