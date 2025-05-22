using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace Notizap.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/informe-global")]
public class InformeController : ControllerBase
{
    private readonly IInformeMensualService _informeService;

    public InformeController(IInformeMensualService informeService)
    {
        _informeService = informeService;
    }

    [HttpGet("pdf")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> ObtenerPdf([FromQuery] int year, [FromQuery] int month, [FromQuery] bool visual = true)
    {
        var resumen = await _informeService.GenerarResumenMensualAsync(year, month);

        var documento = new InformeMensualPdfDocument(resumen, visual);
        var pdfBytes = documento.GeneratePdf();

        var nombreArchivo = $"informe-{year}-{month:D2}.pdf";
        return File(pdfBytes, "application/pdf", nombreArchivo);
    }
}
