public class AnalisisPatronesDto
{
    public string Cuenta { get; set; } = string.Empty;
    public DateTime PeriodoDesde { get; set; }
    public DateTime PeriodoHasta { get; set; }
    public PatronesTemporalesDto PatronesTemporales { get; set; } = new();
    public PatronesContenidoDto PatronesContenido { get; set; } = new();
    public RecomendacionesDto Recomendaciones { get; set; } = new();
}

public class PatronesTemporalesDto
{
    public List<HorarioPerformanceDto> MejoresHorarios { get; set; } = new();
    public List<DiaPerformanceDto> MejoresDias { get; set; } = new();
    public Dictionary<string, double> EngagementPorHora { get; set; } = new();
    public Dictionary<string, double> EngagementPorDia { get; set; } = new();
}

public class HorarioPerformanceDto
{
    public int Hora { get; set; }
    public double EngagementPromedio { get; set; }
    public int TotalPublicaciones { get; set; }
    public string RangoHorario { get; set; } = string.Empty; // "08:00-09:00"
}

public class DiaPerformanceDto
{
    public string DiaSemana { get; set; } = string.Empty;
    public double EngagementPromedio { get; set; }
    public int TotalPublicaciones { get; set; }
    public int OrdenDia { get; set; } // 1=Lunes, 7=Domingo
}

public class PatronesContenidoDto
{
    public List<TipoContenidoAnalisisDto> AnalisisPorTipo { get; set; } = new();
    public List<PalabraClaveDto> PalabrasClaveExitosas { get; set; } = new();
    public DuracionVideoAnalisisDto DuracionVideos { get; set; } = new();
}

public class TipoContenidoAnalisisDto
{
    public string TipoContenido { get; set; } = string.Empty; // "reel", "post", "story"
    public double EngagementPromedio { get; set; }
    public int TotalPublicaciones { get; set; }
    public int AlcancePromedio { get; set; }
    public List<string> TemasRecurrentes { get; set; } = new();
    public double PorcentajeExito { get; set; } // % de publicaciones que superan la media
}

public class PalabraClaveDto
{
    public string Palabra { get; set; } = string.Empty;
    public int Frecuencia { get; set; }
    public double EngagementPromedio { get; set; }
    public double ImpactoPositivo { get; set; } // correlación con mejor performance
}

public class DuracionVideoAnalisisDto
{
    public string RangoDuracionOptimo { get; set; } = string.Empty; // "15-30 segundos"
    public Dictionary<string, double> EngagementPorDuracion { get; set; } = new();
    public double DuracionPromedioExitosos { get; set; }
}

public class RecomendacionesDto
{
    public List<string> HorariosRecomendados { get; set; } = new();
    public List<string> DiasRecomendados { get; set; } = new();
    public List<string> TiposContenidoRecomendados { get; set; } = new();
    public List<string> TemasRecomendados { get; set; } = new();
    public string ProximaAccionSugerida { get; set; } = string.Empty;
    public double ConfianzaRecomendacion { get; set; } // 0-1, qué tan confiable es la recomendación
}