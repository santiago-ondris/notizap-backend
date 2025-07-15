using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Notizap.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly NotizapDbContext _context;
        private readonly ILogger<AuthService> _logger; // Inyección del logger

        public AuthService(
            IConfiguration config, 
            NotizapDbContext context,
            ILogger<AuthService> logger) // Agregar al constructor
        {
            _config = config;
            _context = context;
            _logger = logger; 
        }

        public async Task<UserDto> RegisterAsync(CreateUserDto dto)
        {
            // Log de inicio de registro con validación de entrada
            _logger.LogInformation(
                "🔧 Iniciando registro de usuario: {Email}, Username: {Username}",
                dto.Email,
                dto.Username);

            var existingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (existingUser)
            {
                // Log de intento de registro con email duplicado
                _logger.LogWarning(
                    "⚠️ Intento de registro con email ya existente: {Email}",
                    dto.Email);
                    
                throw new Exception("El email ya está registrado");
            }

            try
            {
                // Log de inicio del proceso de hashing (sin la contraseña)
                _logger.LogDebug("🔐 Iniciando hash de contraseña para usuario: {Email}", dto.Email);
                
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                };

                // Log antes de guardar en BD
                _logger.LogDebug(
                    "💾 Guardando usuario en base de datos: {Email}, Role: {Role}",
                    user.Email,
                    user.Role);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Log de éxito con ID generado
                _logger.LogInformation(
                    "✅ Usuario registrado exitosamente: Id: {UserId}, Email: {Email}, Role: {Role}",
                    user.Id,
                    user.Email,
                    user.Role);

                return new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                };
            }
            catch (Exception ex)
            {
                // Log de error en el proceso de registro
                _logger.LogError(ex,
                    "❌ Error durante registro de usuario: {Email}, Error: {ErrorMessage}",
                    dto.Email,
                    ex.Message);
                    
                throw; // Re-throw para que el controller maneje la respuesta
            }
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request)
        {
            // Log de inicio de autenticación (sin contraseña)
            _logger.LogInformation(
                "🔍 Iniciando autenticación para: {Email}",
                request.Email);

            try
            {
                // Log de búsqueda de usuario
                _logger.LogDebug("🔎 Buscando usuario en base de datos: {Email}", request.Email);
                
                var user = await _context.Users
                    .SingleOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (user == null)
                {
                    // Log de usuario no encontrado
                    _logger.LogWarning(
                        "⚠️ Intento de login con email no registrado: {Email}",
                        request.Email);
                        
                    throw new UnauthorizedAccessException("Credenciales inválidas");
                }

                // Log de verificación de contraseña
                _logger.LogDebug("🔐 Verificando contraseña para usuario: {UserId}", user.Id);
                
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    // ⭐ Log de contraseña incorrecta
                    _logger.LogWarning(
                        "⚠️ Contraseña incorrecta para usuario: {UserId}, Email: {Email}",
                        user.Id,
                        user.Email);
                        
                    throw new UnauthorizedAccessException("Credenciales inválidas");
                }

                // Log de inicio de generación de token
                _logger.LogDebug("🎫 Generando JWT token para usuario: {UserId}", user.Id);
                
                var token = GenerateJwtToken(user);

                // Log de autenticación exitosa
                _logger.LogInformation(
                    "✅ Autenticación exitosa para usuario: {UserId}, Email: {Email}, Role: {Role}",
                    user.Id,
                    user.Email,
                    user.Role);

                return new LoginResponseDto
                {
                    Token = token,
                    Role = user.Role,
                    Username = user.Username,
                    Email = user.Email
                };
            }
            catch (UnauthorizedAccessException)
            {
                // Re-throw sin loggear nuevamente (ya se loggeó arriba)
                throw;
            }
            catch (Exception ex)
            {
                // Log de errores inesperados del sistema
                _logger.LogError(ex,
                    "❌ Error inesperado durante autenticación para: {Email}",
                    request.Email);
                    
                throw;
            }
        }

        private string GenerateJwtToken(User user)
        {
            // Log de generación de token (nivel Debug para no saturar logs)
            _logger.LogDebug(
                "🎫 Generando JWT para usuario: {UserId}, Role: {Role}",
                user.Id,
                user.Role);

            try
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim("email", user.Email), // ⭐ Claim personalizado para logging
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("username", user.Username ?? ""), // ⭐ Para identificar en logs
                    new Claim(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                        ClaimValueTypes.Integer64) // ⭐ Timestamp de emisión
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiry = DateTime.UtcNow.AddMonths(6);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: expiry,
                    signingCredentials: creds
                );

                // Log de token generado exitosamente
                _logger.LogDebug(
                    "✅ JWT generado exitosamente para usuario: {UserId}, Expira: {ExpiryDate}",
                    user.Id,
                    expiry);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // Log de error en generación de token
                _logger.LogError(ex,
                    "❌ Error al generar JWT para usuario: {UserId}",
                    user.Id);
                    
                throw;
            }
        }
    }
}