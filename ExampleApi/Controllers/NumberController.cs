using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExampleApi.Controllers
{
    [Route("api/basic/[controller]")]
    [ApiController]
    public class NumberController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Get()
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
