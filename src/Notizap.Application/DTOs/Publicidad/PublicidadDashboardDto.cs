public class PublicidadDashboardDto
{
    public PublicidadResumenGeneralDto? ResumenGeneral { get; set; }
    public List<PublicidadResumenUnidadDto>? ResumenPorUnidad { get; set; }
    public List<PublicidadResumenPlataformaDto>? ResumenPorPlataforma { get; set; }
    public PublicidadComparativaDto? Comparativa { get; set; }
    public List<TopCampañaDto>? TopCampañas { get; set; }
    public List<TendenciaMensualDto>? TendenciaMensual { get; set; }
}

public class PublicidadResumenGeneralDto
{
    public decimal GastoTotalActual { get; set; }
    public decimal GastoTotalAnterior { get; set; }
    public decimal PorcentajeCambio { get; set; }
    public int TotalCampañasActivas { get; set; }
    public int TotalClicks { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalReach { get; set; }
    public decimal CtrPromedio { get; set; }
    public decimal CostoPromedioPorClick { get; set; }
}

public class PublicidadResumenUnidadDto
{
    public string? UnidadNegocio { get; set; }
    public decimal GastoTotal { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
    public decimal CambioVsMesAnterior { get; set; }
    public int TotalCampañas { get; set; }
    public decimal Performance { get; set; } // CTR * Reach / Gasto
    public int FollowersObtenidos { get; set; }
}

public class PublicidadResumenPlataformaDto
{
    public string? Plataforma { get; set; }
    public decimal GastoTotal { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
    public int TotalCampañas { get; set; }
    public decimal CtrPromedio { get; set; }
    public int TotalClicks { get; set; }
    public int TotalImpressions { get; set; }
    public bool EsAutomatico { get; set; } // Si viene de API o es manual
}

public class PublicidadComparativaDto
{
    public int MesActual { get; set; }
    public int AñoActual { get; set; }
    public int MesAnterior { get; set; }
    public int AñoAnterior { get; set; }
    public decimal DiferenciaGasto { get; set; }
    public decimal PorcentajeCambio { get; set; }
    public string? Tendencia { get; set; } // "aumento", "disminucion", "estable"
    public string? MejorUnidad { get; set; }
    public string? MejorPlataforma { get; set; }
}

public class TopCampañaDto
{
    public string? CampaignId { get; set; }
    public string? Nombre { get; set; }
    public string? UnidadNegocio { get; set; }
    public string? Plataforma { get; set; }
    public decimal MontoInvertido { get; set; }
    public decimal Performance { get; set; }
    public int Clicks { get; set; }
    public int Impressions { get; set; }
    public decimal Ctr { get; set; }
    public int Reach { get; set; }
    public string? TipoFuente { get; set; } // "manual" o "automatico"
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}

public class TendenciaMensualDto
{
    public int Mes { get; set; }
    public int Año { get; set; }
    public string? MesNombre { get; set; }
    public decimal GastoTotal { get; set; }
    public decimal GastoMontella { get; set; }
    public decimal GastoAlenka { get; set; }
    public decimal GastoKids { get; set; }
    public decimal GastoMeta { get; set; }
    public decimal GastoGoogle { get; set; }
    public int TotalCampañas { get; set; }
}

// DTO para los parámetros del dashboard
public class PublicidadDashboardParamsDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? MesesHaciaAtras { get; set; } = 6; // Por defecto últimos 6 meses para tendencias
    public List<string> UnidadesNegocio { get; set; } = new List<string>();
    public List<string> Plataformas { get; set; } = new List<string>();
    public int TopCampañasLimit { get; set; } = 10;
}

// DTO para opciones de filtros disponibles
public class PublicidadDashboardFiltersDto
{
    public List<string> UnidadesNegocio { get; set; } = new List<string>();
    public List<string> Plataformas { get; set; } = new List<string>();
    public DateTime? FechaMinima { get; set; }
    public DateTime? FechaMaxima { get; set; }
}