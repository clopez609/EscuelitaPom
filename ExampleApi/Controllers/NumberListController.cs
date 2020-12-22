using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExampleApi.Controllers
{
    [Authorize]
    [Route("api/jwt/[controller]")]
    [ApiController]
    public class NumberListController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            Random rnd = new Random();
            List<int> values = new List<int>();

            await Task.Run(() =>
            {
                while (values.Count <= 10)
                {
                    int randomNumber = rnd.Next(1, 10);

                    values.Add(randomNumber);
                }
            });

            return Ok(values);
        }
    }
}
