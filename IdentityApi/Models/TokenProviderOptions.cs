using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Models
{
    public class TokenProviderOptions
    {
        public string Path { get; set; } = "/api/token";
        public string Issuer { get; set; } = "https://localhost:44349/";
        public string Audience { get; set; } = "https://localhost:44349/";
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);
    }
}
