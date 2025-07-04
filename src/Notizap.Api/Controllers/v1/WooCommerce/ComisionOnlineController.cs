using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "admin,superadmin")]
[ApiController]
[Route("api/v1/comisiones-online")]
public class ComisionOnlineController : ControllerBase
{
    private readonly IComisionOnlineService _comisionService;
    private readonly ILogger<ComisionOnlineController> _logger;

    public ComisionOnlineController(
        IComisionOnlineService comisionService,
        ILogger<ComisionOnlineController> logger)
    {
        _comisionService = comisionService;
        _logger = logger;
    }

    // GET: api/v1/comisiones-online
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComisionOnlineDto>>> GetAll()
    {
        try
        {
            var comisiones = await _comisionService.GetAllAsync();
            return Ok(comisiones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las comisiones");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/v1/comisiones-online/paged
    [HttpGet("paged")]
    public async Task<ActionResult<object>> GetPaged([FromQuery] ComisionOnlineQueryDto query)
    {
        try
        {
            var (items, totalCount) = await _comisionService.GetPagedAsync(query);
            
            var result = new
            {
                items,
                totalCount,
                pageNumber = query.PageNumber,
                pageSize = query.PageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener comisiones paginadas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/v1/comisiones-online/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ComisionOnlineDto>> GetById(int id)
    {
        try
        {
            var comision = await _comisionService.GetByIdAsync(id);
            return Ok(comision);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Comisión con ID {id} no encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener comisión por ID {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/v1/comisiones-online/periodo/{mes}/{año}
    [HttpGet("periodo/{mes}/{año}")]
    public async Task<ActionResult<ComisionOnlineDto>> GetByPeriodo(int mes, int año)
    {
        try
        {
            var comision = await _comisionService.GetByPeriodoAsync(mes, año);
            
            if (comision == null)
                return NotFound($"No se encontró comisión para {mes:00}/{año}");

            return Ok(comision);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener comisión por período {Mes}/{Año}", mes, año);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/v1/comisiones-online/año/{año}
    [HttpGet("año/{año}")]
    public async Task<ActionResult<IEnumerable<ComisionOnlineDto>>> GetByAño(int año)
    {
        try
        {
            var comisiones = await _comisionService.GetByAñoAsync(año);
            return Ok(comisiones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener comisiones por año {Año}", año);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/v1/comisiones-online
    [HttpPost]
    public async Task<ActionResult<ComisionOnlineDto>> Create([FromBody] CreateComisionOnlineDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comision = await _comisionService.CreateAsync(createDto);
            
            return CreatedAtAction(
                nameof(GetById), 
                new { id = comision.Id }, 
                comision);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear comisión");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/v1/comisiones-online/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<ComisionOnlineDto>> Update(int id, [FromBody] UpdateComisionOnlineDto updateDto)
    {
        try
        {
            if (id != updateDto.Id)
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comision = await _comisionService.UpdateAsync(updateDto);
            return Ok(comision);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Comisión con ID {id} no encontrada");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar comisión {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/v1/comisiones-online/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _comisionService.DeleteAsync(id);
            
            if (!result)
                return NotFound($"Comisión con ID {id} no encontrada");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar comisión {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/v1/comisiones-online/periodo/{mes}/{año}
    [HttpDelete("periodo/{mes}/{año}")]
    public async Task<ActionResult> DeleteByPeriodo(int mes, int año)
    {
        try
        {
            var result = await _comisionService.DeleteByPeriodoAsync(mes, año);
            
            if (!result)
                return NotFound($"No se encontró comisión para {mes:00}/{año}");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar comisión por período {Mes}/{Año}", mes, año);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/v1/comisiones-online/calcular
    [HttpPost("calcular")]
    public ActionResult<CalculoComisionDto> CalcularComision([FromBody] CalcularComisionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var calculo = _comisionService.CalcularComision(
                request.Mes,
                request.Año,
                request.TotalSinNC,
                request.MontoAndreani,
                request.MontoOCA,
                request.MontoCaddy);

            return Ok(calculo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular comisión");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/v1/comisiones-online/exists/periodo/{mes}/{año}
    [HttpGet("exists/periodo/{mes}/{año}")]
    public async Task<ActionResult<object>> ExistsByPeriodo(int mes, int año)
    {
        try
        {
            var exists = await _comisionService.ExistsAsync(mes, año);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia por período {Mes}/{Año}", mes, año);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/v1/comisiones-online/auxiliares
    [HttpGet("auxiliares")]
    public async Task<ActionResult<object>> GetAuxiliares()
    {
        try
        {
            var años = await _comisionService.GetAñosDisponiblesAsync();
            
            return Ok(new { años });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos auxiliares");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}

// DTO adicional para el endpoint de cálculo
public class CalcularComisionRequestDto
{
    public int Mes { get; set; }
    public int Año { get; set; }
    public decimal TotalSinNC { get; set; }
    public decimal MontoAndreani { get; set; }
    public decimal MontoOCA { get; set; }
    public decimal MontoCaddy { get; set; }
}