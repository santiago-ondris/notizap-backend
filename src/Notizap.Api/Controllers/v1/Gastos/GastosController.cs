using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Notizap.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/gastos")]
    public class GastosController : ControllerBase
    {
        private readonly IGastoService _gastoService;

        public GastosController(IGastoService gastoService)
        {
            _gastoService = gastoService;
        }

        [HttpGet]
        [Authorize(Roles = "viewer,admin,superadmin")]
        [SwaggerOperation(Summary = "Obtener todos los gastos")]
        public async Task<IActionResult> ObtenerTodos()
        {
            var gastos = await _gastoService.ObtenerTodosAsync();
            return Ok(gastos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "viewer,admin,superadmin")]
        [SwaggerOperation(Summary = "Obtener gasto por Id")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var gasto = await _gastoService.ObtenerPorIdAsync(id);
            return gasto is null ? NotFound() : Ok(gasto);
        }

        [HttpPost]
        [Authorize(Roles = "admin,superadmin")]
        [SwaggerOperation(Summary = "Crear un gasto")]
        public async Task<IActionResult> Crear([FromBody] CreateGastoDto dto)
        {
            var nuevoGasto = await _gastoService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoGasto.Id }, nuevoGasto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,superadmin")]
        [SwaggerOperation(Summary = "Actualizar gasto")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] UpdateGastoDto dto)
        {
            var actualizado = await _gastoService.ActualizarAsync(id, dto);
            return actualizado is null ? NotFound() : Ok(actualizado);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,superadmin")]
        [SwaggerOperation(Summary = "Eliminar un gasto")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _gastoService.EliminarAsync(id);
            return eliminado ? NoContent() : NotFound();
        }
    }
}
