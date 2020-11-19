using IdentityApi.Models;
using IdentityApi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityApi.Services
{
    public interface IIdentityService
    {
        Claim[] GetIdentity(InputBodyViewModel model);
        TokenResponse GenerateToken(Claim[] claim);
    }
}
