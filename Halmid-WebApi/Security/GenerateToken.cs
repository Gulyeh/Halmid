using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Halmid_WebApi.Controllers
{
    public class GenerateToken
    {
        private static readonly string SALT = "b697fb300b7f9f6826f0aceace0f5480scb697fb300b7f9f6826f0aceace0f5480=b697fb300b7f9f6826f0aceace0f5480scb697fb300b7f9f6826f0aceace0f5480=b697fb300b7f9f6826f0aceace0f5480scb697fb300b7f9f6826f0aceace0f5480=b697fb300b7f9f6826f0aceace0f5480scb697fb300b7f9f6826f0aceace0f5480=";
        private static readonly SymmetricSecurityKey TOKEN_KEY = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SALT));
        
        public static string JwtGenerate()
        {
            var tokenhandler = new JwtSecurityTokenHandler();
            var token = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(TOKEN_KEY, SecurityAlgorithms.HmacSha256)
            };
            var _Token = tokenhandler.CreateToken(token);
            return tokenhandler.WriteToken(_Token);
        }
    }
}
