using IdentityApi.Models;
using IdentityApi.ViewModels;
using Microsoft.Extensions.Configuration;
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
    public class IdentityService : IIdentityService
    {
        private static readonly string secretKey = "SecretKeywqewqeqqqqqqqqqqqweeeeeeeeeeeeeeeeeeeqweqe";
        private readonly IConfiguration _configuration;

        public IdentityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Claim[] GetIdentity(InputBodyViewModel model)
        {
            //string username = "admin";
            //string password = "admin123";
            //if (username == "admin" && password == "admin123")

            string body = string.Join(",", model.Method, model.Channel, model.Path);

            if(!string.IsNullOrEmpty(body))
            {
                return new Claim[] {
                        new Claim("input-body", body)
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
                    _configuration["Issuer"],
                    _configuration["Audience"],
                    claim,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddMinutes(1)
                );

                //Token
                var jwt = new JwtSecurityToken(header, payload);

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                var response = new TokenResponse()
                {
                    AccessToken = encodedJwt,
                    ExpiresIn = (int)TimeSpan.FromMinutes(1).TotalSeconds
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
