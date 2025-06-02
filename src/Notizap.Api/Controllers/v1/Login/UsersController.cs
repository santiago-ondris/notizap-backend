using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize(Roles = "superadmin")]
public class UsersController : ControllerBase
{
    private readonly NotizapDbContext _context;

    public UsersController(NotizapDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Obtiene todos los usuarios de la DB")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            })
            .ToListAsync();

        return Ok(users);
    }

    // 2. Cambiar rol de usuario (solo admin o viewer, nunca superadmin)
    [HttpPut("{id}/role")]
    [SwaggerOperation(Summary = "Cambio el rol del usuario")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        if (dto.Role != "admin" && dto.Role != "viewer")
            return BadRequest(new { message = "Rol no permitido." });

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        // Proteger para que nadie se pueda dar rol superadmin por API
        if (user.Role == "superadmin")
            return BadRequest(new { message = "No se puede modificar el rol de un superadmin." });

        user.Role = dto.Role;
        await _context.SaveChangesAsync();

        return NoContent();
    }
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Elimina un usuario (excepto superadmin)")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado." });

        if (user.Role == "superadmin")
            return BadRequest(new { message = "No se puede eliminar un superadmin." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
