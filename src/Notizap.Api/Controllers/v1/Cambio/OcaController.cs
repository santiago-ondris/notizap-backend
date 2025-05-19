using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Notizap.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/oca")]
[Authorize]
public class OcaController : ControllerBase
{
    private readonly IOcaService _ocaService;
    private readonly IHttpClientFactory _httpClientFactory;

    public OcaController(IOcaService ocaService, IHttpClientFactory httpClientFactory)
    {
        _ocaService = ocaService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("generar-etiqueta/{cambioId}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Genera la etiqueta de envío en OCA para un cambio.")]
    public async Task<ActionResult<OcaEnvioResponseDto>> GenerarEtiqueta(int cambioId)
    {
        var resultado = await _ocaService.GenerarEtiquetaAsync(cambioId);
        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpGet("sucursales")]
    [Authorize(Roles = "admin,superadmin,viewer")]
    [SwaggerOperation(Summary = "Devuelve las sucursales OCA disponibles para un código postal.")]
    public async Task<ActionResult<List<SucursalOcaDto>>> ObtenerSucursales([FromQuery] string cp)
    {
        var client = _httpClientFactory.CreateClient();
        string url = $"https://ocaepakweb.oca.com.ar/Oep_Web/OepSucursales.svc/json/GetCentrosImposicionPorCP?cp={cp}";

        try
        {
            var response = await client.GetFromJsonAsync<List<SucursalOcaDto>>(url);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al consultar OCA: {ex.Message}");
        }
    }
}
