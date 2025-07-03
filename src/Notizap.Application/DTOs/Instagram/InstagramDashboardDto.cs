public class InstagramDashboardDto
{
    public string Cuenta { get; set; } = string.Empty;
    public MetricasGeneralesDto MetricasGenerales { get; set; } = new();
    public List<EvolucionSeguidoresDto> EvolucionSeguidores { get; set; } = new();
    public ComparativaContenidoDto ComparativaContenido { get; set; } = new();
    public List<TopContentItemDto> TopPerformers { get; set; } = new();
    public DateTime UltimaSincronizacion { get; set; }
}

public class MetricasGeneralesDto
{
    public int SeguidoresActuales { get; set; }
    public int CrecimientoSemanal { get; set; }
    public double PorcentajeCrecimientoSemanal { get; set; }
    public int TotalPublicacionesPeriodo { get; set; }
    public double EngagementPromedio { get; set; }
    public int TotalInteracciones { get; set; }
    public int AlcancePromedio { get; set; }
}

public class EvolucionSeguidoresDto
{
    public DateTime Fecha { get; set; }
    public int Seguidores { get; set; }
    public int CrecimientoDiario { get; set; }
}

public class ComparativaContenidoDto
{
    public ContenidoMetricasDto Reels { get; set; } = new();
    public ContenidoMetricasDto Posts { get; set; } = new();
    public ContenidoMetricasDto Stories { get; set; } = new();
}

public class ContenidoMetricasDto
{
    public int TotalPublicaciones { get; set; }
    public double EngagementPromedio { get; set; }
    public int AlcancePromedio { get; set; }
    public int InteraccionesPromedio { get; set; }
    public int MejorPerformance { get; set; } // valor de la mejor métrica
    public string TipoMetricaMejor { get; set; } = string.Empty; // "views", "likes", etc.
}

public class TopContentItemDto
{
    public string Id { get; set; } = string.Empty;
    public string TipoContenido { get; set; } = string.Empty; // "reel", "post", "story"
    public DateTime FechaPublicacion { get; set; }
    public string? ImageUrl { get; set; }
    public string? Url { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public int MetricaPrincipal { get; set; } // la métrica por la que está en el top
    public string NombreMetrica { get; set; } = string.Empty;
    public double EngagementRate { get; set; }
    public Dictionary<string, object> MetricasAdicionales { get; set; } = new();
}