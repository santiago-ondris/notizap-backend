using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes")]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClienteController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Obtener todos los clientes")]
    public async Task<ActionResult<PagedResult<ClienteResumenDto>>> GetAll(
        int pageNumber = 1, int pageSize = 20)
    {
        var result = await _clienteService.GetAllAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtener cliente por ID")]
    public async Task<ActionResult<ClienteDetalleDto>> GetById(int id)
    {
        var cliente = await _clienteService.GetByIdAsync(id);
        if (cliente == null) return NotFound();
        return Ok(cliente);
    }

    [HttpGet("ranking")]
    [SwaggerOperation(Summary = "Obtener clientes por monto consumido")]
    public async Task<ActionResult<List<ClienteResumenDto>>> GetRanking(
        [FromQuery] string ordenarPor = "monto", 
        [FromQuery] int top = 10,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? canal = null,
        [FromQuery] string? sucursal = null,
        [FromQuery] string? marca = null,
        [FromQuery] string? categoria = null)
    {
        try 
        {
            var ranking = await _clienteService.GetRankingAsync(ordenarPor, top, desde, hasta, canal, sucursal, marca, categoria);
            return Ok(ranking);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en ranking: {ex.Message}");
            return BadRequest($"Error al obtener ranking: {ex.Message}");
        }
    }

    [HttpGet("buscar")]
    [SwaggerOperation(Summary = "Buscar clientes por nombre")]
    public async Task<ActionResult<List<ClienteResumenDto>>> BuscarPorNombre([FromQuery] string nombre)
    {
        var clientes = await _clienteService.BuscarPorNombreAsync(nombre);
        return Ok(clientes);
    }

    [HttpGet("filtrar")]
    [SwaggerOperation(Summary = "Filtrado de clientes")]
    public async Task<ActionResult<PagedResult<ClienteResumenDto>>> Filtrar(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? canal = null,
        [FromQuery] string? sucursal = null,
        [FromQuery] string? marca = null,
        [FromQuery] string? categoria = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12)
    {
        try 
        {
            var clientes = await _clienteService.FiltrarAsync(desde, hasta, canal, sucursal, marca, categoria, pageNumber, pageSize);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            // Log del error para debugging
            Console.WriteLine($"Error en filtrar: {ex.Message}");
            return BadRequest($"Error al filtrar clientes: {ex.Message}");
        }
    }

    [HttpPatch("{id}/inactivo")]
    [SwaggerOperation(Summary = "Marcar un cliente como inactivo")]
    public async Task<IActionResult> MarcarInactivo(int id)
    {
        await _clienteService.MarcarInactivoAsync(id);
        return NoContent();
    }
    [HttpGet("filtros/canales")]
    public async Task<ActionResult<List<string>>> GetCanalesDisponibles()
    {
        var canales = await _clienteService.GetCanalesDisponiblesAsync();
        return Ok(canales);
    }

    [HttpGet("filtros/sucursales")]
    public async Task<ActionResult<List<string>>> GetSucursalesDisponibles()
    {
        var sucursales = await _clienteService.GetSucursalesDisponiblesAsync();
        return Ok(sucursales);
    }

    [HttpGet("filtros/marcas")]
    public async Task<ActionResult<List<string>>> GetMarcasDisponibles()
    {
        var marcas = await _clienteService.GetMarcasDisponiblesAsync();
        return Ok(marcas);
    }

    [HttpGet("filtros/categorias")]
    public async Task<ActionResult<List<string>>> GetCategoriasDisponibles()
    {
        var categorias = await _clienteService.GetCategoriasDisponiblesAsync();
        return Ok(categorias);
    }
}
