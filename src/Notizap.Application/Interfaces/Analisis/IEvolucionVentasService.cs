using Microsoft.AspNetCore.Http;

public interface IEvolucionVentasService
    {
        EvolucionVentasResponse CalcularEvolucionVentas(IFormFile archivoVentas);
        EvolucionVentasResumenResponse CalcularEvolucionVentasResumen(IFormFile archivoVentas);
    }