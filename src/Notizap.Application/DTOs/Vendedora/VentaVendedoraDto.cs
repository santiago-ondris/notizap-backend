using Microsoft.AspNetCore.Http;
public class VentaVendedoraDto
{
    public int Id { get; set; }
    public string SucursalNombre { get; set; } = string.Empty;
    public string VendedorNombre { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int Cantidad { get; set; }
    public int CantidadReal { get; set; } // Cantidad ajustada para productos especiales
    public decimal Total { get; set; }
    public string Turno { get; set; } = string.Empty;
    public bool EsProductoDescuento { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public bool SucursalAbreSabadoTarde { get; set; }
}

public class VentaVendedoraCreateDto
{
    public int SucursalId { get; set; }
    public int VendedorId { get; set; }
    public string SucursalNombre { get; set; } = string.Empty;
    public string VendedorNombre { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}

public class VentaVendedoraUploadDto
{
    public IFormFile Archivo { get; set; } = null!;
    public bool SobreescribirDuplicados { get; set; } = false;
}