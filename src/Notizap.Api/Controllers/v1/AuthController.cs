using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Notizap.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = _authService.Authenticate(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Credenciales inválidas");
            }
        }
    }
}
