using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Notizap.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        // Lista de usuarios mockeados
        private readonly List<User> _users = new()
        {
            new User { Email = "viewer@notizap.com", Password = "viewer123", Role = "viewer" },
            new User { Email = "admin@notizap.com", Password = "admin123", Role = "admin" },
            new User { Email = "superadmin@notizap.com", Password = "superadmin123", Role = "superadmin" }
        };

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public LoginResponseDto Authenticate(LoginRequestDto request)
        {
            var user = _users.SingleOrDefault(u =>
                u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == request.Password);

            if (user == null)
                throw new UnauthorizedAccessException("Credenciales inválidas");

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token,
                Role = user.Role
            };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
