using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class TokenGenerator
    {
        public static string GenerateTestToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();


            var key = Encoding.UTF8.GetBytes("FOKJdnfsdjnfsjdNJSDNFsjdfsdnfjsdnfJKSNDfjsjfn");
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


            var userClaims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "C4725F3E-E024-46D9-B93F-4F6AD8BEC02A"),
        new Claim(ClaimTypes.Name, "testuser@example.com"),
        new Claim(ClaimTypes.Email, "testuser@example.com"),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = credentials,
                Issuer = "https://localhost:7253",
                Audience = "https://localhost:7253"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
