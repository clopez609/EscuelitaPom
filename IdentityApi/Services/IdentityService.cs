using IdentityApi.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityApi.Services
{
    public class IdentityService
    {
        private static readonly string secretKey = "SecretKeywqewqeqqqqqqqqqqqweeeeeeeeeeeeeeeeeeeqweqe";
        private readonly TokenProviderOptions _tokenProviderOptions;

        public IdentityService()
        {
        }

        public Claim[] GetIdentity(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                return new Claim[] {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, "Admin"),
                };
            }
            return null;
        }

        public TokenResponse GenerateToken(Claim[] claim)
        {
            try
            {
                //Header 
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
                var header = new JwtHeader(signingCredentials);

                //Paylod
                var payload = new JwtPayload(
                    _tokenProviderOptions.Issuer,
                    _tokenProviderOptions.Audience,
                    claim,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddMinutes(5)
                );

                //Token
                var jwt = new JwtSecurityToken(header, payload);

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                var response = new TokenResponse()
                {
                    AccessToken = encodedJwt,
                    ExpiresIn = (int)_tokenProviderOptions.Expiration.TotalSeconds
                };

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
    }
}
