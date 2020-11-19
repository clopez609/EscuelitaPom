using ExampleApi.Helpers;
using IdentityApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            AuthenticateBasic();

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllBasic);
            var values = JsonConvert.DeserializeObject<List<int>>(await response.Content.ReadAsStringAsync());

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<List<int>>(values);
        }

        [Fact]
        public async Task Get_Without_AuthBasic()
        {
            // Arrange
            // No declaramos la auth basic.

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllBasic);
            var values = JsonConvert.DeserializeObject<List<int>>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Null(values);
        }

        [Fact]
        public async Task Get_With_Jwt()
        {
            // Arrange
            AuthenticateJwt();

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllJwt);
            var values = JsonConvert.DeserializeObject<List<int>>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<List<int>>(values);
        }

        [Fact]
        public async Task Get_Without_Jwt()
        {
            // Arrange
            // No declaramos la auth jwt.

            // Act
            var response = await _testClient.GetAsync(ApiRoutes.Numbers.GetAllJwt);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
