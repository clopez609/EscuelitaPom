using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Models
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
