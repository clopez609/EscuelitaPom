using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExampleApi.Controllers
{
    [Route("api/basic/[controller]")]
    [ApiController]
    public class NumberController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            Random rnd = new Random();
            List<int> values = new List<int>();

            while (values.Count <= 10)
            {
                int randomNumber = rnd.Next(1, 10);
                               
                values.Add(randomNumber);
            }

            return Ok(values);
        }
    }
}
