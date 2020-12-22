using ExampleApi.Helpers;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ExampleApi.IntegrationTests
{
    public class NumberControllerTest : IntegrationTest
    {
        [Fact]
        public async Task Get_With_AuthBasic()
        {
            // Arrange
            ClearHeaderRequest();
            AuthenticateBasic();

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllBasic);
            var content = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<int>>(content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNullOrWhiteSpace();
            values.Should().NotBeNull();
        }

        [Fact]
        public async Task Get_Without_Credentials_AuthBasic()
        {
            // Arrange
            ClearHeaderRequest();
            AuthenticateBasicWithoutCredentials(); // Sin usuario y contraseña

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllBasic);
            var content = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<int>>(content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().BeNullOrWhiteSpace();
            values.Should().BeNull();
        }

        [Fact]
        public async Task Get_Without_AuthBasic()
        {
            // Arrange
            // No declaramos la auth basic.
            ClearHeaderRequest();

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllBasic);
            var content = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<int>>(content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().BeNullOrWhiteSpace();
            values.Should().BeNull();
        }

        [Fact]
        public async Task Get_With_Jwt()
        {
            // Arrange
            ClearHeaderRequest();
            AuthenticateJwt();

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllJwt);
            var content = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<int>>(content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNullOrWhiteSpace();
            values.Should().NotBeNull();
        }

        [Fact]
        public async Task Get_Without_Jwt()
        {
            // Arrange
            // No declaramos la auth jwt.
            ClearHeaderRequest();

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllJwt);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Get_Without_Jwt_Bearer_Empty()
        {
            // Arrange
            ClearHeaderRequest();
            AuthenticateWithoutJwt(); // Bearer sin token

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllJwt);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Get_Without_Issuer_Jwt()
        {
            // Arrange
            ClearHeaderRequest();
            AuthenticateWithoutIssuerJwt(); // Token sin emisor declarado

            // Act
            Func<Task> response = async () => await _testClient.GetAsync(ApiRoutes.Numbers.GetAllJwt);

            // Assert
            await response.Should().ThrowAsync<SecurityTokenInvalidIssuerException>();
        }

        [Fact]
        public async Task Get_Jwt_Expires()
        {
            // Arrange
            ClearHeaderRequest();
            AuthenticateWithJwtExpires(); // Token expirado

            // Act
            Func<Task> response = async () => await _testClient.GetAsync(ApiRoutes.Numbers.GetAllJwt);

            // Assert
            await response.Should().ThrowAsync<SecurityTokenExpiredException>();
        }
    }
}
