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

        [HttpPost("filtrar")]
        [SwaggerOperation(Summary = "Obtiene gastos aplicando filtros de búsqueda, paginado.")]
        public async Task<ActionResult<object>> ObtenerConFiltros([FromBody] GastoFiltrosDto filtros)
        {
            var (gastos, totalCount) = await _gastoService.ObtenerConFiltrosAsync(filtros);

            return Ok(new 
            { 
                gastos, 
                totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / filtros.TamañoPagina)
            });
        }

        [HttpGet("resumen")]
        [SwaggerOperation(Summary = "Obtiene un resumen mensual de gastos por año y mes.")]
        public async Task<ActionResult<GastoResumenDto>> ObtenerResumenMensual([FromQuery] int año, [FromQuery] int mes)
        {
            var resumen = await _gastoService.ObtenerResumenMensualAsync(año, mes);
            return Ok(resumen);
        }

        [HttpGet("por-categoria")]
        [SwaggerOperation(Summary = "Obtiene un desglose de gastos por categoría entre fechas opcionales.")]
        public async Task<ActionResult<IEnumerable<GastoPorCategoriaDto>>> ObtenerGastosPorCategoria(
            [FromQuery] DateTime? desde = null, 
            [FromQuery] DateTime? hasta = null)
        {
            var gastos = await _gastoService.ObtenerGastosPorCategoriaAsync(desde, hasta);
            return Ok(gastos);
        }

        [HttpGet("categorias")]
        [SwaggerOperation(Summary = "Devuelve todas las categorías de gastos disponibles.")]
        public async Task<ActionResult<IEnumerable<string>>> ObtenerCategorias()
        {
            var categorias = await _gastoService.ObtenerCategoriasAsync();
            return Ok(categorias);
        }

        [HttpGet("tendencia")]
        [SwaggerOperation(Summary = "Obtiene la tendencia mensual de gastos de los últimos X meses.")]
        public async Task<ActionResult<IEnumerable<GastoTendenciaDto>>> ObtenerTendenciaMensual([FromQuery] int meses = 12)
        {
            var tendencia = await _gastoService.ObtenerTendenciaMensualAsync(meses);
            return Ok(tendencia);
        }

        [HttpGet("recurrentes")]
        [SwaggerOperation(Summary = "Obtiene la lista de gastos recurrentes registrados.")]
        public async Task<ActionResult<IEnumerable<GastoDto>>> ObtenerGastosRecurrentes()
        {
            var gastos = await _gastoService.ObtenerGastosRecurrentesAsync();
            return Ok(gastos);
        }

        [HttpGet("top")]
        [SwaggerOperation(Summary = "Obtiene los gastos más altos dentro de un rango de fechas opcional.")]
        public async Task<ActionResult<IEnumerable<GastoDto>>> ObtenerTopGastos(
            [FromQuery] int cantidad = 5,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var gastos = await _gastoService.ObtenerTopGastosAsync(cantidad, desde, hasta);
            return Ok(gastos);
        }
    }
}
