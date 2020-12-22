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

        protected void ClearHeaderRequest()
        {
            _testClient.DefaultRequestHeaders.Authorization = null;
        }

        protected void AuthenticateBasic()
        {
            _testClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetBasicAsync());
        }

        protected void AuthenticateBasicWithoutCredentials()
        {
            _testClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", " ");
        }

        protected void AuthenticateJwt()
        {
            _testClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetJwtAsync());
        }

        protected void AuthenticateWithoutIssuerJwt()
        {
            _testClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetJwtWithoutIssuerAsync());
        }

        protected void AuthenticateWithoutJwt()
        {
            _testClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", " ");
        }

        protected void AuthenticateWithJwtExpires()
        {
            _testClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsIm5iZiI6MTYwNjM5Nzk5MCwiZXhwIjoxNjA2Mzk4MDUwLCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MDAxLyIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0OjUwMDEvIn0.yzULAsvBqBf4ocuNNMjvwx4NZRlKsvr1vwGCXjFviuo");
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

        private string GetJwtWithoutIssuerAsync()
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
                    "",
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
