using ExampleApi.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ExampleApi.IntegrationTests
{
    public class ControllerUnitTest
    {
        DefaultHttpContext _httpContext;
        Mock<IConfigurationSection> _configurationSection;
        Mock<IConfiguration> _configuration;
        Mock<RequestDelegate> _next;
        public ControllerUnitTest()
        {
            _httpContext = new DefaultHttpContext();
            _configurationSection = new Mock<IConfigurationSection>();
            _configuration = new Mock<IConfiguration>();
            _next = new Mock<RequestDelegate>();
        }

        [Theory]
        [InlineData("admin", "admin123")]
        public async Task ExecuteInvoke_WithBasicAuthorizationGood_NotThrowException(string username, string password)
        {
            // Arrange
            _configurationSection.Setup(opt => opt.Value).Returns(username);
            _configuration.Setup(v => v["USER"]).Returns(_configurationSection.Object.Value);

            _configurationSection.Setup(opt => opt.Value).Returns(password);
            _configuration.Setup(v => v["PASS"]).Returns(_configurationSection.Object.Value);

            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            _httpContext.Request.Headers["Authorization"] = $"Basic {credentials}";

            var middleware = new MiddlewareSecurityAuth(_next.Object, _configuration.Object);

            // Act
            Func<Task> task = async () => await middleware.Invoke(_httpContext);

            // Assert
            await task.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ExecuteInvoke_WithBasicAuthorizationBad_ThrowException()
        {
            // Arrange
            _httpContext.Request.Headers["Authorization"] = $"Basic ";
            var middleware = new MiddlewareSecurityAuth(_next.Object, _configuration.Object);

            // Act
            Func<Task> task = async () => await middleware.Invoke(_httpContext);

            // Assert
            await task.Should().ThrowAsync<NotImplementedException>();
        }

        [Theory]
        [InlineData("SecretKeywqewqeqqqqqqqqqqqweeeeeeeeeeeeeeeeeeeqweqe", "https://localhost:7001/")]
        public async Task ExecuteInvoke_WithJwtAuthorizationGood_NotThrowException(string token, string issuer)
        {
            // Arrange
            _configurationSection.Setup(opt => opt.Value).Returns(token);
            _configuration.Setup(v => v["SECRET_KEY"]).Returns(_configurationSection.Object.Value);

            _configurationSection.Setup(opt => opt.Value).Returns(issuer);
            _configuration.Setup(v => v["ISSUER"]).Returns(_configurationSection.Object.Value);

            _httpContext.Request.Headers["Authorization"] = $"Bearer {GetJwt()}";

            var middlewae = new MiddlewareSecurityAuth(_next.Object, _configuration.Object);

            // Act
            Func<Task> task = async () => await middlewae.Invoke(_httpContext);

            // Assert
            await task.Should().NotThrowAsync();
        }

        private static string GetJwt()
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
    }
}
