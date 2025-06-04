using Microsoft.AspNetCore.Http;

public class EvolucionVentasRequest
    {
        public IFormFile ArchivoVentas { get; set; } = null!;
    }