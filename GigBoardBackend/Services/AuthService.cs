using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using GigBoardBackend.Models;

namespace GigBoardBackend.Services
{
	public class AuthService : IAuthService
	{
		public AuthService()
		{

		}

		public string GenerateToken(User user)
		{
			var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
				?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is not set.");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>()
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Username)
			};

			var token = new JwtSecurityToken(
				issuer: "GigBoardBackend",
				audience: "GigBoardBackend",
				claims: claims,
				expires: DateTime.Now.AddHours(1),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}