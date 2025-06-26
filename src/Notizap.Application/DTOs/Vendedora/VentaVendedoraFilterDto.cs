public class VentaVendedoraFilterDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? SucursalNombre { get; set; }
    public string? VendedorNombre { get; set; }
    public string? Turno { get; set; } // "Mañana", "Tarde", null = ambos
    public decimal? MontoMinimo { get; set; }
    public decimal? MontoMaximo { get; set; }
    public int? CantidadMinima { get; set; }
    public int? CantidadMaxima { get; set; }
    public bool IncluirProductosDescuento { get; set; } = true;
    public bool ExcluirDomingos { get; set; } = true;
    public string OrderBy { get; set; } = "fecha"; // fecha, vendedor, sucursal, total, cantidad
    public bool OrderDesc { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class VentaVendedoraStatsDto
{
    public int TotalVentas { get; set; }
    public decimal MontoTotal { get; set; }
    public int CantidadTotal { get; set; }
    public decimal PromedioVentaPorDia { get; set; }
    public decimal PromedioVentaPorVendedora { get; set; }
    public int DiasConVentas { get; set; }
    
    public List<VentaPorVendedoraDto> TopVendedoras { get; set; } = new();
    
    public List<VentaPorVendedoraDto> TodasVendedoras { get; set; } = new();
    
    public List<VentaPorSucursalDto> VentasPorSucursal { get; set; } = new();
    public List<VentaPorTurnoDto> VentasPorTurno { get; set; } = new();
    public List<VentaPorDiaDto> VentasPorDia { get; set; } = new();
}

public class VentaPorVendedoraDto
{
    public string VendedorNombre { get; set; } = string.Empty;
    public int TotalVentas { get; set; }
    public decimal MontoTotal { get; set; }
    public int CantidadTotal { get; set; }
    public decimal Promedio { get; set; }
    public List<string> SucursalesQueTrabaja { get; set; } = new();
}

public class VentaPorSucursalDto
{
    public string SucursalNombre { get; set; } = string.Empty;
    public int TotalVentas { get; set; }
    public decimal MontoTotal { get; set; }
    public int CantidadTotal { get; set; }
    public bool AbreSabadoTarde { get; set; }
}

public class VentaPorTurnoDto
{
    public string Turno { get; set; } = string.Empty;
    public int TotalVentas { get; set; }
    public decimal MontoTotal { get; set; }
    public int CantidadTotal { get; set; }
}

public class VentaPorDiaDto
{
    public DateTime Fecha { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public int TotalVentas { get; set; }
    public decimal MontoTotal { get; set; }
    public int CantidadTotal { get; set; }
    public bool EsDomingo { get; set; }
}