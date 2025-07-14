using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Notizap.Api.Controllers.Cambios
{
    [ApiController]
    [Route("api/v1/cambios/mercadolibre/devoluciones")]
    [Authorize]
    public class DevolucionesMercadoLibreController : ControllerBase
    {
        private readonly IDevolucionMercadoLibreService _service;
        private readonly ILogger<DevolucionesMercadoLibreController> _logger;

        public DevolucionesMercadoLibreController(
            IDevolucionMercadoLibreService service,
            ILogger<DevolucionesMercadoLibreController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las devoluciones de MercadoLibre
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "viewer,admin,superadmin")]
        public async Task<ActionResult<IEnumerable<DevolucionMercadoLibreDto>>> GetAll()
        {
            try
            {
                var devoluciones = await _service.GetAllAsync();
                return Ok(devoluciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las devoluciones de MercadoLibre");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene una devolución por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "viewer,admin,superadmin")]
        public async Task<ActionResult<DevolucionMercadoLibreDto>> GetById(int id)
        {
            try
            {
                var devolucion = await _service.GetByIdAsync(id);
                
                if (devolucion == null)
                    return NotFound($"No se encontró la devolución con ID {id}");

                return Ok(devolucion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la devolución {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene devoluciones filtradas
        /// </summary>
        [HttpPost("filtrar")]
        [Authorize(Roles = "viewer,admin,superadmin")]
        public async Task<ActionResult<IEnumerable<DevolucionMercadoLibreDto>>> GetFiltered(
            [FromBody] DevolucionMercadoLibreFiltrosDto filtros)
        {
            try
            {
                var devoluciones = await _service.GetFilteredAsync(filtros);
                return Ok(devoluciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener devoluciones filtradas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea una nueva devolución
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult<DevolucionMercadoLibreDto>> Create(
            [FromBody] CreateDevolucionMercadoLibreDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var devolucion = await _service.CreateAsync(dto);
                
                return CreatedAtAction(
                    nameof(GetById), 
                    new { id = devolucion.Id }, 
                    devolucion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la devolución");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza una devolución existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult<DevolucionMercadoLibreDto>> Update(
            int id, 
            [FromBody] UpdateDevolucionMercadoLibreDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var devolucion = await _service.UpdateAsync(id, dto);
                
                if (devolucion == null)
                    return NotFound($"No se encontró la devolución con ID {id}");

                return Ok(devolucion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la devolución {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza solo el estado de nota de crédito
        /// </summary>
        [HttpPatch("{id}/nota-credito")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult> UpdateNotaCredito(
            int id, 
            [FromBody] UpdateNotaCreditoDto dto) 
        {
            try
            {
                var resultado = await _service.UpdateNotaCreditoAsync(id, dto.NotaCreditoEmitida);  
                
                if (!resultado)
                    return NotFound($"No se encontró la devolución con ID {id}");

                return Ok(new { 
                    message = "Estado de nota de crédito actualizado correctamente",
                    id = id,
                    notaCreditoEmitida = dto.NotaCreditoEmitida  
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar nota de crédito {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPatch("{id}/traslado")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult> UpdateTraslado(
            int id,
            [FromBody] UpdateTrasladoDto dto)
        {
            try
            {
                var resultado = await _service.UpdateTrasladoAsync(id, dto.Trasladado);

                if(!resultado)
                {
                    return NotFound($"No se encontró la devolución con ID {id}");
                }

                return Ok(new {
                    message = "Estado de traslado actualizado correctamente",
                    id = id,
                    trasladado = dto.Trasladado
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar traslado {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }


        /// <summary>
        /// Elimina una devolución
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _service.DeleteAsync(id);
                
                if (!resultado)
                    return NotFound($"No se encontró la devolución con ID {id}");

                return Ok(new { 
                    message = "Devolución eliminada correctamente",
                    id = id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la devolución {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene estadísticas generales
        /// </summary>
        [HttpGet("estadisticas")]
        [Authorize(Roles = "viewer,admin,superadmin")]
        public async Task<ActionResult<DevolucionMercadoLibreEstadisticasDto>> GetEstadisticas()
        {
            try
            {
                var estadisticas = await _service.GetEstadisticasAsync();
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene estadísticas filtradas
        /// </summary>
        [HttpPost("estadisticas/filtrar")]
        [Authorize(Roles = "viewer,admin,superadmin")]
        public async Task<ActionResult<DevolucionMercadoLibreEstadisticasDto>> GetEstadisticasFiltered(
            [FromBody] DevolucionMercadoLibreFiltrosDto filtros)
        {
            try
            {
                var estadisticas = await _service.GetEstadisticasFilteredAsync(filtros);
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas filtradas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene los meses disponibles para filtros
        /// </summary>
        [HttpGet("meses-disponibles")]
        [Authorize(Roles = "viewer,admin,superadmin")]
        public async Task<ActionResult<IEnumerable<object>>> GetMesesDisponibles()
        {
            try
            {
                var meses = await _service.GetMesesDisponiblesAsync();
                
                var resultado = meses.Select(m => new 
                {
                    año = m.Año,
                    mes = m.Mes,
                    nombreMes = m.NombreMes,
                    valor = $"{m.Año}-{m.Mes:D2}",
                    etiqueta = $"{m.NombreMes} {m.Año}"
                });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener meses disponibles");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Verifica si existe una devolución
        /// </summary>
        [HttpHead("{id}")]
        [Authorize(Roles = "viewer,admin,superadmin")]
        public async Task<ActionResult> Exists(int id)
        {
            try
            {
                var existe = await _service.ExistsAsync(id);
                return existe ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de devolución {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}