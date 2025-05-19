using Microsoft.Extensions.Options;
using System.Xml.Linq;

public class OcaService : IOcaService
{
    private readonly NotizapDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OcaSettings _settings;

    public OcaService(
        NotizapDbContext context,
        IHttpClientFactory httpClientFactory,
        IOptions<OcaSettings> settings)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
    }

    public async Task<OcaEnvioResponseDto> GenerarEtiquetaAsync(int cambioId)
    {
        var cambio = await _context.Cambios.FindAsync(cambioId);
        if (cambio == null)
        {
            return new OcaEnvioResponseDto
            {
                Exito = false,
                MensajeError = "Cambio no encontrado."
            };
        }

        cambio.IdCentroImposicionDestino = _settings.IdCentroImposicionDestino;

        // Generar XML
        string xml = OcaXmlBuilder.GenerarXmlEnvio(cambio, _settings.NumeroCuenta);
        Console.WriteLine(xml);

        var client = _httpClientFactory.CreateClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("usr", _settings.Usuario),
            new KeyValuePair<string, string>("psw", _settings.Password),
            new KeyValuePair<string, string>("XML_Datos", xml),
            new KeyValuePair<string, string>("ConfirmarRetiro", "true"),
            new KeyValuePair<string, string>("ArchivoCliente", ""),   
            new KeyValuePair<string, string>("ArchivoProceso", "")  
        });

        try
        {
            var response = await client.PostAsync("http://webservice.oca.com.ar/ePak_tracking/Oep_TrackEPak.asmx/IngresoORMultiplesRetiros", content);

            if (!response.IsSuccessStatusCode)
            {
                return new OcaEnvioResponseDto
                {
                    Exito = false,
                    MensajeError = $"OCA devolvió status: {response.StatusCode}"
                };
            }

            string xmlResponse = await response.Content.ReadAsStringAsync();

            // Parsear el XML devuelto
            var xdoc = XDocument.Parse(xmlResponse);
            var detalleIngreso = xdoc.Descendants("DetalleIngresos").FirstOrDefault();
            string? tracking = detalleIngreso?.Element("NumeroEnvio")?.Value;
            string? ordenRetiroStr = detalleIngreso?.Element("OrdenRetiro")?.Value;

            if (string.IsNullOrWhiteSpace(tracking) || string.IsNullOrWhiteSpace(ordenRetiroStr))
            {
                return new OcaEnvioResponseDto
                {
                    Exito = false,
                    MensajeError = "No se pudo obtener el número de envío o el número de orden de retiro."
                };
            }

            int ordenRetiro = int.Parse(ordenRetiroStr);

            if (tracking == null)
            {
                var errorNode = xdoc.Descendants("DetalleRechazos").FirstOrDefault();
                string? motivo = errorNode?.Element("Motivo")?.Value;
                return new OcaEnvioResponseDto
                {
                    Exito = false,
                    MensajeError = motivo ?? "OCA devolvió respuesta sin tracking."
                };
            }

            cambio.TrackingNumber = tracking;
            cambio.IdOrdenRetiro = ordenRetiro;
            cambio.UrlEtiquetaPdf = $"https://www5.oca.com.ar/OcaEPakNet/Views/Impresiones/Etiquetas.aspx?IdOrdenRetiro={ordenRetiro}";
            cambio.YaEnviado = true;

            await _context.SaveChangesAsync();

            return new OcaEnvioResponseDto
            {
                Exito = true,
                TrackingNumber = tracking,
                UrlEtiquetaPdf = cambio.UrlEtiquetaPdf,
            };
        }
        catch (Exception ex)
        {
            return new OcaEnvioResponseDto
            {
                Exito = false,
                MensajeError = $"Error interno: {ex.Message}"
            };
        }
    }
}
