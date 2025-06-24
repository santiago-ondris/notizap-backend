public class DevolucionMercadoLibreDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public bool NotaCreditoEmitida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string Pedido { get; set; } = string.Empty;
}

public class CreateDevolucionMercadoLibreDto
{
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public bool NotaCreditoEmitida { get; set; } = false;
    public string Pedido { get; set; } = string.Empty;
}

public class UpdateDevolucionMercadoLibreDto
{
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public bool NotaCreditoEmitida { get; set; }
    public string Pedido { get; set; } = string.Empty;
}

public class DevolucionMercadoLibreFiltrosDto
{
    public int? Año { get; set; }
    public int? Mes { get; set; }
    public string? Cliente { get; set; }
    public string? Modelo { get; set; }
    public bool? NotaCreditoEmitida { get; set; }
    public string? Pedido { get; set; }
}

public class DevolucionMercadoLibreEstadisticasDto
{
    public int TotalDevoluciones { get; set; }
    public int NotasCreditoEmitidas { get; set; }
    public int NotasCreditoPendientes { get; set; }
    public int DevolucionesMesActual { get; set; }
    public decimal PorcentajeNotasEmitidas { get; set; }
    
    // Estadísticas por mes para gráficos
    public List<EstadisticasMensualDto> EstadisticasPorMes { get; set; } = new();
}

public class EstadisticasMensualDto
{
    public int Año { get; set; }
    public int Mes { get; set; }
    public string NombreMes { get; set; } = string.Empty;
    public int TotalDevoluciones { get; set; }
    public int NotasCreditoEmitidas { get; set; }
    public int NotasCreditoPendientes { get; set; }
}
