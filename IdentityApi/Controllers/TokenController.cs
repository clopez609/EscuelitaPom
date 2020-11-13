using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityApi.Services;
using IdentityApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        public TokenController()
        {
        }

        [HttpGet]
        public IActionResult Get(IdentityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var service = new IdentityService();
            var userClaims = service.GetIdentity(model.Username, model.Password);

            if (userClaims == null) return BadRequest();

            var token = service.GenerateToken(userClaims);

            return Ok(token);
        }
    }
}
