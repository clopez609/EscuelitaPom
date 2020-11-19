using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
