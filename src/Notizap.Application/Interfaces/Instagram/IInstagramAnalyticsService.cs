public interface IInstagramAnalyticsService
{
    /// <summary>
    /// Obtiene el dashboard principal con métricas agregadas
    /// </summary>
    Task<InstagramDashboardDto> GetDashboardAsync(string cuenta, DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Analiza patrones temporales y de contenido para generar insights
    /// </summary>
    Task<AnalisisPatronesDto> GetAnalisisPatronesAsync(string cuenta, DateTime desde, DateTime hasta);

    /// <summary>
    /// Obtiene métricas de evolución de seguidores con análisis de crecimiento
    /// </summary>
    Task<List<EvolucionSeguidoresDto>> GetEvolucionSeguidoresDetallada(string cuenta, DateTime desde, DateTime hasta);

    /// <summary>
    /// Calcula el engagement rate promedio por tipo de contenido
    /// </summary>
    Task<Dictionary<string, double>> GetEngagementRatePorTipoAsync(string cuenta, DateTime desde, DateTime hasta);

    /// <summary>
    /// Obtiene los mejores horarios de publicación basado en engagement histórico
    /// </summary>
    Task<List<HorarioPerformanceDto>> GetMejoresHorariosAsync(string cuenta, int diasAnalisis = 30);

    /// <summary>
    /// Genera un resumen ejecutivo para reportes
    /// </summary>
    Task<ResumenEjecutivoDto> GetResumenEjecutivoAsync(string cuenta, DateTime desde, DateTime hasta);

    /// <summary>
    /// Obtiene métricas comparativas entre períodos
    /// </summary>
    Task<ComparativaPeriodosDto> GetComparativaPeriodosAsync(string cuenta, DateTime periodoActualDesde, 
        DateTime periodoActualHasta, DateTime periodoAnteriorDesde, DateTime periodoAnteriorHasta);
}

// DTOs adicionales para los nuevos métodos
public class ResumenEjecutivoDto
{
    public string Cuenta { get; set; } = string.Empty;
    public DateTime PeriodoAnalizado { get; set; }
    public string TendenciaGeneral { get; set; } = string.Empty; // "Creciente", "Estable", "Decreciente"
    public List<string> LogrosPrincipales { get; set; } = new();
    public List<string> AreasDeOportunidad { get; set; } = new();
    public List<string> AccionesRecomendadas { get; set; } = new();
    public Dictionary<string, string> MetricasClave { get; set; } = new(); // "Crecimiento": "+15%"
}

public class ComparativaPeriodosDto
{
    public string Cuenta { get; set; } = string.Empty;
    public PeriodoMetricasDto PeriodoActual { get; set; } = new();
    public PeriodoMetricasDto PeriodoAnterior { get; set; } = new();
    public CambiosPeriodoDto Cambios { get; set; } = new();
}

public class PeriodoMetricasDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int Seguidores { get; set; }
    public double EngagementPromedio { get; set; }
    public int TotalPublicaciones { get; set; }
    public int TotalInteracciones { get; set; }
    public int AlcancePromedio { get; set; }
}

public class CambiosPeriodoDto
{
    public int CambioSeguidores { get; set; }
    public double PorcentajeCambioSeguidores { get; set; }
    public double CambioEngagement { get; set; }
    public double PorcentajeCambioEngagement { get; set; }
    public int CambioPublicaciones { get; set; }
    public double PorcentajeCambioPublicaciones { get; set; }
    public string TendenciaGeneral { get; set; } = string.Empty;
    public List<string> InsightsClaves { get; set; } = new();
}