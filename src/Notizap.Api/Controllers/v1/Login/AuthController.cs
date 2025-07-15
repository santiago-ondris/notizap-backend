using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Notizap.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly NotizapDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger; // Inyección del logger

        public AuthController(
            IAuthService authService, 
            NotizapDbContext context, 
            IEmailService emailService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;
            _logger = logger; 
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerOperation(Summary = "Inicio de sesion")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Log de inicio con información relevante (SIN datos sensibles)
            _logger.LogInformation(
                "🔐 Intento de login iniciado para email: {Email} desde IP: {IpAddress}",
                request.Email,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            try
            {
                var response = await _authService.AuthenticateAsync(request);
                
                // Log de éxito con contexto de usuario
                _logger.LogInformation(
                    "✅ Login exitoso para usuario: {Email}, Rol: {Role}, SessionId: {SessionId}",
                    request.Email,
                    response.Role,
                    HttpContext.TraceIdentifier);

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Log de intento de acceso no autorizado (importante para seguridad)
                _logger.LogWarning(
                    "⚠️ Intento de login fallido para email: {Email}, IP: {IpAddress}, Razón: {Reason}",
                    request.Email,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    ex.Message);

                return Unauthorized("Credenciales inválidas");
            }
            catch (Exception ex)
            {
                // Log de errores inesperados
                _logger.LogError(ex,
                    "❌ Error inesperado durante login para email: {Email}, SessionId: {SessionId}",
                    request.Email,
                    HttpContext.TraceIdentifier);

                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "superadmin")]
        [SwaggerOperation(Summary = "Registro de usuario")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            // Log de inicio de registro con contexto del admin que lo hace
            var adminEmail = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "unknown";
            
            _logger.LogInformation(
                "👤 Inicio de registro de usuario. Admin: {AdminEmail}, NuevoUsuario: {NewUserEmail}",
                adminEmail,
                dto.Email);

            try
            {
                var user = await _authService.RegisterAsync(dto);
                
                // Log de registro exitoso - importante para auditoría
                _logger.LogInformation(
                    "✅ Usuario registrado exitosamente. Id: {UserId}, Email: {Email}, Role: {Role}, CreadoPor: {AdminEmail}",
                    user.Id,
                    user.Email,
                    user.Role,
                    adminEmail);

                return Ok(user);
            }
            catch (Exception ex)
            {
                // Log de error en registro con contexto completo
                _logger.LogError(ex,
                    "❌ Error al registrar usuario. Email: {Email}, Admin: {AdminEmail}, Error: {ErrorMessage}",
                    dto.Email,
                    adminEmail,
                    ex.Message);

                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Contraseña olvidada")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            // Log de solicitud de reset (sin revelar si el usuario existe)
            _logger.LogInformation(
                "🔑 Solicitud de reset de contraseña para: {Email}, IP: {IpAddress}",
                dto.Email,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);

            // Siempre responde igual por seguridad, pero si existe genera token
            if (user != null)
            {
                var token = Guid.NewGuid().ToString("N");
                user.PasswordResetToken = token;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();

                var resetLink = $"http://localhost:5173/reset-password?token={token}";
                
                try
                {
                    await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
                    
                    // Log de envío exitoso de email (sin el token por seguridad)
                    _logger.LogInformation(
                        "📧 Email de reset enviado exitosamente para usuario: {UserId}, Email: {Email}",
                        user.Id,
                        user.Email);
                }
                catch (Exception ex)
                {
                    // Log de error al enviar email
                    _logger.LogError(ex,
                        "❌ Error al enviar email de reset para usuario: {UserId}, Email: {Email}",
                        user.Id,
                        user.Email);
                }
            }
            else
            {
                // Log de intento con email no registrado (para detectar ataques)
                _logger.LogWarning(
                    "⚠️ Solicitud de reset para email no registrado: {Email}, IP: {IpAddress}",
                    dto.Email,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            }

            // Siempre la misma respuesta por seguridad
            return Ok(new { message = "Si existe una cuenta asociada, recibirás instrucciones para restablecer tu contraseña a tu mail." });
        }

        // Endpoint para obtener info de logs (solo para superadmin en desarrollo)
        [HttpGet("health")]
        [Authorize(Roles = "superadmin")]
        [SwaggerOperation(Summary = "Health check con info de logging")]
        public IActionResult HealthCheck()
        {
            _logger.LogInformation("🔍 Health check ejecutado por: {AdminEmail}", 
                User.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "unknown");

            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                logging = "Serilog configured",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }
    }
}