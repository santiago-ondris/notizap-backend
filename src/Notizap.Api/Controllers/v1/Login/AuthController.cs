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

        public AuthController(IAuthService authService, NotizapDbContext context, IEmailService emailService)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerOperation(Summary = "Inicio de sesion")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await _authService.AuthenticateAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Credenciales inválidas");
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            try
            {
                var user = await _authService.RegisterAsync(dto);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);

            // Siempre responde igual por seguridad, pero si existe genera token
            if (user != null)
            {
                var token = Guid.NewGuid().ToString("N");
                user.PasswordResetToken = token;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();

                var resetLink = $"http://localhost:5173/reset-password?token={token}";
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
            }

            return Ok(new { message = "Si existe una cuenta asociada, recibirás instrucciones para restablecer tu contraseña a tu mail." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u =>
                u.PasswordResetToken == dto.Token &&
                u.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return BadRequest(new { message = "Token inválido o expirado." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña restablecida correctamente." });
        }
    }
}
