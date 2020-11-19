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
        private readonly IdentityService _identityService;

        public TokenController(IdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost]
        public IActionResult Post(InputBodyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userClaims = _identityService.GetIdentity(model);

            if (userClaims == null) return BadRequest();

            var token = _identityService.GenerateToken(userClaims);

            return Ok(token);
        }
    }
}
