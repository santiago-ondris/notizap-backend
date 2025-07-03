using System.Text.RegularExpressions;
public static class InstagramAnalyticsHelper
{
    /// <summary>
    /// Calcula el engagement rate para cualquier tipo de contenido
    /// </summary>
    public static double CalcularEngagementRate(int interacciones, int seguidores, int? alcance = null)
    {
        if (seguidores <= 0) return 0;
        
        // Si tenemos alcance, usar alcance como denominador (más preciso)
        var denominador = alcance ?? seguidores;
        if (denominador <= 0) return 0;
        
        return Math.Round((double)interacciones / denominador * 100, 2);
    }

    /// <summary>
    /// Calcula el crecimiento porcentual entre dos valores
    /// </summary>
    public static double CalcularCrecimientoPorcentual(int valorActual, int valorAnterior)
    {
        if (valorAnterior <= 0) return valorActual > 0 ? 100 : 0;
        return Math.Round(((double)(valorActual - valorAnterior) / valorAnterior) * 100, 2);
    }

    /// <summary>
    /// Determina la tendencia basada en datos históricos
    /// </summary>
    public static string DeterminarTendencia(List<int> valores)
    {
        if (valores.Count < 2) return "Insuficientes datos";

        var mitad = valores.Count / 2;
        var primeraMitad = valores.Take(mitad).Average();
        var segundaMitad = valores.Skip(mitad).Average();

        var diferencia = (segundaMitad - primeraMitad) / primeraMitad * 100;

        return diferencia switch
        {
            > 5 => "Creciente",
            < -5 => "Decreciente",
            _ => "Estable"
        };
    }

    /// <summary>
    /// Extrae palabras clave del contenido y calcula su frecuencia
    /// </summary>
    public static List<PalabraClaveDto> ExtractPalabrasClaveConEngagement<T>(
        List<T> contenidos, 
        Func<T, string> getContenido, 
        Func<T, double> getEngagement)
    {
        var palabrasContador = new Dictionary<string, (int frecuencia, List<double> engagements)>();

        foreach (var item in contenidos)
        {
            var contenido = getContenido(item)?.ToLower() ?? "";
            var engagement = getEngagement(item);
            
            // Extraer palabras (mínimo 3 caracteres, sin caracteres especiales)
            var palabras = Regex.Matches(contenido, @"\b[a-záéíóúñü]{3,}\b")
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(p => !EsPalabraComun(p))
                .Distinct();

            foreach (var palabra in palabras)
            {
                if (!palabrasContador.ContainsKey(palabra))
                    palabrasContador[palabra] = (0, new List<double>());
                
                palabrasContador[palabra] = (
                    palabrasContador[palabra].frecuencia + 1,
                    palabrasContador[palabra].engagements.Concat(new[] { engagement }).ToList()
                );
            }
        }

        return palabrasContador
            .Where(p => p.Value.frecuencia >= 2) // Mínimo 2 apariciones
            .Select(p => new PalabraClaveDto
            {
                Palabra = p.Key,
                Frecuencia = p.Value.frecuencia,
                EngagementPromedio = Math.Round(p.Value.engagements.Average(), 2),
                ImpactoPositivo = CalcularImpactoPositivo(p.Value.engagements, 
                    contenidos.Select(getEngagement).Average())
            })
            .OrderByDescending(p => p.ImpactoPositivo)
            .Take(10)
            .ToList();
    }

    /// <summary>
    /// Agrupa contenido por horarios y calcula performance
    /// </summary>
    public static List<HorarioPerformanceDto> CalcularPerformancePorHorario<T>(
        List<T> contenidos,
        Func<T, DateTime> getFecha,
        Func<T, double> getEngagement)
    {
        return contenidos
            .GroupBy(c => getFecha(c).Hour)
            .Select(g => new HorarioPerformanceDto
            {
                Hora = g.Key,
                EngagementPromedio = Math.Round(g.Select(getEngagement).Average(), 2),
                TotalPublicaciones = g.Count(),
                RangoHorario = $"{g.Key:D2}:00-{(g.Key + 1):D2}:00"
            })
            .Where(h => h.TotalPublicaciones >= 2) // Mínimo 2 publicaciones para ser significativo
            .OrderByDescending(h => h.EngagementPromedio)
            .ToList();
    }

    /// <summary>
    /// Agrupa contenido por día de la semana y calcula performance
    /// </summary>
    public static List<DiaPerformanceDto> CalcularPerformancePorDia<T>(
        List<T> contenidos,
        Func<T, DateTime> getFecha,
        Func<T, double> getEngagement)
    {
        var nombresDias = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };

        return contenidos
            .GroupBy(c => getFecha(c).DayOfWeek)
            .Select(g => new DiaPerformanceDto
            {
                DiaSemana = nombresDias[(int)g.Key],
                OrdenDia = (int)g.Key,
                EngagementPromedio = Math.Round(g.Select(getEngagement).Average(), 2),
                TotalPublicaciones = g.Count()
            })
            .Where(d => d.TotalPublicaciones >= 1)
            .OrderByDescending(d => d.EngagementPromedio)
            .ToList();
    }

    /// <summary>
    /// Genera insights automáticos basado en datos
    /// </summary>
    public static List<string> GenerarInsights(
        double engagementActual, 
        double engagementAnterior,
        int seguidoresActuales,
        int seguidoresAnteriores,
        List<HorarioPerformanceDto> mejoresHorarios)
    {
        var insights = new List<string>();

        // Insight sobre engagement
        var cambioEngagement = engagementActual - engagementAnterior;
        if (Math.Abs(cambioEngagement) > 0.5)
        {
            var direccion = cambioEngagement > 0 ? "aumentó" : "disminuyó";
            insights.Add($"El engagement {direccion} {Math.Abs(cambioEngagement):F1}% respecto al período anterior");
        }

        // Insight sobre seguidores
        var crecimientoSeguidores = CalcularCrecimientoPorcentual(seguidoresActuales, seguidoresAnteriores);
        if (Math.Abs(crecimientoSeguidores) > 2)
        {
            var direccion = crecimientoSeguidores > 0 ? "crecimiento" : "decrecimiento";
            insights.Add($"Hubo un {direccion} de {Math.Abs(crecimientoSeguidores):F1}% en seguidores");
        }

        // Insight sobre horarios
        if (mejoresHorarios.Any())
        {
            var mejorHorario = mejoresHorarios.First();
            insights.Add($"Las {mejorHorario.RangoHorario} es el mejor horario de publicación con {mejorHorario.EngagementPromedio}% de engagement");
        }

        return insights;
    }

    /// <summary>
    /// Genera recomendaciones basadas en análisis
    /// </summary>
    public static List<string> GenerarRecomendaciones(
        AnalisisPatronesDto analisis,
        double engagementObjetivo = 3.0)
    {
        var recomendaciones = new List<string>();

        // Recomendaciones de horarios
        var topHorarios = analisis.PatronesTemporales.MejoresHorarios.Take(3);
        if (topHorarios.Any())
        {
            recomendaciones.Add($"Publicar entre {string.Join(", ", topHorarios.Select(h => h.RangoHorario))} para mayor engagement");
        }

        // Recomendaciones de días
        var topDias = analisis.PatronesTemporales.MejoresDias.Take(2);
        if (topDias.Any())
        {
            recomendaciones.Add($"Los {string.Join(" y ", topDias.Select(d => d.DiaSemana.ToLower()))} son los mejores días para publicar");
        }

        // Recomendaciones de contenido
        var mejorTipo = analisis.PatronesContenido.AnalisisPorTipo
            .OrderByDescending(t => t.EngagementPromedio)
            .FirstOrDefault();
        
        if (mejorTipo != null)
        {
            recomendaciones.Add($"Enfocar más en {mejorTipo.TipoContenido} que genera {mejorTipo.EngagementPromedio:F1}% de engagement");
        }

        return recomendaciones;
    }

    // Métodos privados auxiliares
    private static bool EsPalabraComun(string palabra)
    {
        var palabrasComunes = new HashSet<string> 
        { 
            "que", "con", "para", "por", "una", "los", "las", "del", "como", "más", 
            "muy", "todo", "sobre", "cuando", "donde", "desde", "hasta", "entre" 
        };
        return palabrasComunes.Contains(palabra);
    }

    private static double CalcularImpactoPositivo(List<double> engagementsConPalabra, double promedioGeneral)
    {
        var promedioConPalabra = engagementsConPalabra.Average();
        return Math.Round(((promedioConPalabra - promedioGeneral) / promedioGeneral) * 100, 2);
    }
}