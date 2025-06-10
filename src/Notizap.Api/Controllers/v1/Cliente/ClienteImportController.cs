using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes/import")]
public class ClienteImportController : ControllerBase
{
    private readonly IClienteImportService _importService;

    public ClienteImportController(IClienteImportService importService)
    {
        _importService = importService;
    }

    [HttpPost]
    [Authorize(Roles = "superadmin")]
    [SwaggerOperation(Summary = "Cargar archivo XLSX de clientes")]
    [ProducesResponseType(typeof(ImportacionClientesDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ImportacionClientesDto>> Importar([FromForm] ImportArchivoRequest request)
    {
        if (request.Archivo == null || request.Archivo.Length == 0)
            return BadRequest("Debe adjuntar un archivo Excel válido.");

        using var stream = request.Archivo.OpenReadStream();
        var result = await _importService.ImportarDesdeExcelAsync(stream, request.Archivo.FileName);

        return Ok(result);
    }

    [HttpPost("validar")]
    [Authorize(Roles = "superadmin")]
    [SwaggerOperation(Summary = "Prevalidar el archivo por cargar")]
    [ProducesResponseType(typeof(List<string>), 200)]
    [ProducesResponseType(400)]
    public ActionResult<List<string>> Validar([FromForm] ImportArchivoRequest request)
    {
        if (request.Archivo == null || request.Archivo.Length == 0)
            return BadRequest("Debe adjuntar un archivo Excel válido.");

        using var stream = request.Archivo.OpenReadStream();
        var errores = _importService.ValidarArchivo(stream);

        if (errores.Count == 0)
            return Ok(new List<string>()); // Sin errores

        return BadRequest(errores);
    }
}
