using ExampleApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using Xunit;

namespace ExampleApi.UnitTest
{
    public class NumberTest
    {
        [Fact]
        public async void GetBasicAll()
        {
            var controller = new NumberController();

            var result = await controller.Get();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void GetJwtAll()
        {
            var controller = new NumberListController();

            var result = await controller.Get();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
