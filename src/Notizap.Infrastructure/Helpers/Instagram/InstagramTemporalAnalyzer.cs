public static class InstagramTemporalAnalyzer
{
    /// <summary>
    /// Analiza patrones temporales de publicación y engagement
    /// </summary>
    public static PatronesTemporalesDto AnalizarPatronesTemporales(
        List<InstagramReel> reels, 
        List<InstagramPost> posts, 
        List<InstagramStory> stories, 
        int seguidores)
    {
        // Combinar todo el contenido para análisis temporal
        var todosContenidos = new List<(DateTime fecha, double engagement)>();

        // Agregar reels
        todosContenidos.AddRange(reels.Select(r => (
            r.FechaPublicacion, 
            InstagramAnalyticsHelper.CalcularEngagementRate(r.Interacciones, seguidores, r.Reach)
        )));

        // Agregar posts
        todosContenidos.AddRange(posts.Select(p => (
            p.FechaPublicacion,
            InstagramAnalyticsHelper.CalcularEngagementRate((int)p.Interactions, seguidores, p.Reach)
        )));

        // Agregar stories
        todosContenidos.AddRange(stories.Select(s => (
            s.FechaPublicacion,
            InstagramAnalyticsHelper.CalcularEngagementRate(s.Replies, seguidores, s.Reach)
        )));

        var mejoresHorarios = CalcularMejoresHorarios(todosContenidos);
        var mejoresDias = CalcularMejoresDias(todosContenidos);

        // Calcular engagement por hora para gráfico
        var engagementPorHora = todosContenidos
            .GroupBy(t => t.fecha.Hour)
            .ToDictionary(
                g => g.Key.ToString("D2") + ":00",
                g => Math.Round(g.Average(x => x.engagement), 2)
            );

        // Calcular engagement por día
        var engagementPorDia = todosContenidos
            .GroupBy(t => t.fecha.DayOfWeek)
            .ToDictionary(
                g => GetNombreDia(g.Key),
                g => Math.Round(g.Average(x => x.engagement), 2)
            );

        return new PatronesTemporalesDto
        {
            MejoresHorarios = mejoresHorarios,
            MejoresDias = mejoresDias,
            EngagementPorHora = engagementPorHora,
            EngagementPorDia = engagementPorDia
        };
    }

    /// <summary>
    /// Calcula los mejores horarios de publicación
    /// </summary>
    public static List<HorarioPerformanceDto> CalcularMejoresHorarios(
        List<(DateTime fecha, double engagement)> contenidos)
    {
        return contenidos
            .GroupBy(c => c.fecha.Hour)
            .Where(g => g.Count() >= 2) // Mínimo 2 publicaciones para ser significativo
            .Select(g => new HorarioPerformanceDto
            {
                Hora = g.Key,
                EngagementPromedio = Math.Round(g.Average(x => x.engagement), 2),
                TotalPublicaciones = g.Count(),
                RangoHorario = $"{g.Key:D2}:00-{(g.Key + 1):D2}:00"
            })
            .OrderByDescending(h => h.EngagementPromedio)
            .Take(8) // Top 8 horarios
            .ToList();
    }

    /// <summary>
    /// Calcula los mejores días de la semana para publicar
    /// </summary>
    public static List<DiaPerformanceDto> CalcularMejoresDias(
        List<(DateTime fecha, double engagement)> contenidos)
    {
        return contenidos
            .GroupBy(c => c.fecha.DayOfWeek)
            .Select(g => new DiaPerformanceDto
            {
                DiaSemana = GetNombreDia(g.Key),
                OrdenDia = (int)g.Key,
                EngagementPromedio = Math.Round(g.Average(x => x.engagement), 2),
                TotalPublicaciones = g.Count()
            })
            .Where(d => d.TotalPublicaciones >= 1)
            .OrderByDescending(d => d.EngagementPromedio)
            .ToList();
    }

    /// <summary>
    /// Analiza los mejores horarios para un período específico
    /// </summary>
    public static List<HorarioPerformanceDto> AnalizarMejoresHorarios(
        List<InstagramReel> reels,
        List<InstagramPost> posts,
        int seguidores,
        int diasAnalisis = 30)
    {
        var fecha = DateTime.UtcNow.AddDays(-diasAnalisis);
        
        var reelsFiltrados = reels.Where(r => r.FechaPublicacion >= fecha).ToList();
        var postsFiltrados = posts.Where(p => p.FechaPublicacion >= fecha).ToList();

        var todoContenido = new List<(DateTime fecha, double engagement)>();
        
        todoContenido.AddRange(reelsFiltrados.Select(r => (
            r.FechaPublicacion,
            InstagramAnalyticsHelper.CalcularEngagementRate(r.Interacciones, seguidores, r.Reach)
        )));

        todoContenido.AddRange(postsFiltrados.Select(p => (
            p.FechaPublicacion,
            InstagramAnalyticsHelper.CalcularEngagementRate((int)p.Interactions, seguidores, p.Reach)
        )));

        return CalcularMejoresHorarios(todoContenido);
    }

    /// <summary>
    /// Genera evolución detallada de seguidores con crecimiento diario
    /// </summary>
    public static List<EvolucionSeguidoresDto> ProcesarEvolucionSeguidores(
        List<(DateTime fecha, int seguidores)> seguidoresPorDia)
    {
        var resultado = new List<EvolucionSeguidoresDto>();
        var seguidoresAnterior = 0;

        foreach (var dia in seguidoresPorDia.OrderBy(s => s.fecha))
        {
            var crecimiento = seguidoresAnterior > 0 ? dia.seguidores - seguidoresAnterior : 0;
            
            resultado.Add(new EvolucionSeguidoresDto
            {
                Fecha = dia.fecha,
                Seguidores = dia.seguidores,
                CrecimientoDiario = crecimiento
            });

            seguidoresAnterior = dia.seguidores;
        }

        return resultado;
    }

    /// <summary>
    /// Genera insights automáticos basado en datos temporales
    /// </summary>
    public static List<string> GenerarInsightsTempo(
        List<HorarioPerformanceDto> mejoresHorarios,
        List<DiaPerformanceDto> mejoresDias,
        double engagementPromedio)
    {
        var insights = new List<string>();

        // Insight sobre horarios
        if (mejoresHorarios.Any())
        {
            var mejorHorario = mejoresHorarios.First();
            if (mejorHorario.EngagementPromedio > engagementPromedio * 1.2)
            {
                insights.Add($"Las {mejorHorario.RangoHorario} generan {mejorHorario.EngagementPromedio}% de engagement, 20% superior al promedio");
            }
        }

        // Insight sobre días
        if (mejoresDias.Any())
        {
            var mejorDia = mejoresDias.First();
            var peorDia = mejoresDias.Last();
            var diferencia = mejorDia.EngagementPromedio - peorDia.EngagementPromedio;
            
            if (diferencia > 1.0)
            {
                insights.Add($"Los {mejorDia.DiaSemana.ToLower()} superan a los {peorDia.DiaSemana.ToLower()} por {diferencia:F1}% en engagement");
            }
        }

        // Insight sobre consistencia
        if (mejoresHorarios.Count >= 3)
        {
            var variacionHorarios = mejoresHorarios.Take(3).Select(h => h.EngagementPromedio).ToList();
            var desviacion = CalcularDesviacionEstandar(variacionHorarios);
            
            if (desviacion < 0.5)
            {
                insights.Add("El engagement es consistente en los mejores horarios de publicación");
            }
            else
            {
                insights.Add("Hay una gran variación en el engagement según el horario de publicación");
            }
        }

        return insights;
    }

    /// <summary>
    /// Genera recomendaciones basadas en análisis temporal
    /// </summary>
    public static List<string> GenerarRecomendacionesTempo(
        List<HorarioPerformanceDto> mejoresHorarios,
        List<DiaPerformanceDto> mejoresDias,
        int totalPublicaciones)
    {
        var recomendaciones = new List<string>();

        // Recomendaciones de horarios
        var topHorarios = mejoresHorarios.Take(3);
        if (topHorarios.Any())
        {
            recomendaciones.Add($"Publicar entre {string.Join(", ", topHorarios.Select(h => h.RangoHorario))} para maximizar engagement");
        }

        // Recomendaciones de días
        var topDias = mejoresDias.Take(2);
        if (topDias.Any())
        {
            recomendaciones.Add($"Enfocar publicaciones en {string.Join(" y ", topDias.Select(d => d.DiaSemana.ToLower()))}");
        }

        // Recomendación sobre frecuencia
        if (totalPublicaciones < 20)
        {
            recomendaciones.Add("Aumentar la frecuencia de publicación para obtener datos más precisos");
        }
        else if (totalPublicaciones > 100)
        {
            recomendaciones.Add("Mantener la consistencia en horarios que han demostrado mejor performance");
        }

        return recomendaciones;
    }

    // Métodos auxiliares privados
    private static string GetNombreDia(DayOfWeek dia)
    {
        return dia switch
        {
            DayOfWeek.Sunday => "Domingo",
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miércoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sábado",
            _ => dia.ToString()
        };
    }

    private static double CalcularDesviacionEstandar(List<double> valores)
    {
        if (!valores.Any()) return 0;
        
        var promedio = valores.Average();
        var sumaCuadrados = valores.Sum(v => Math.Pow(v - promedio, 2));
        return Math.Sqrt(sumaCuadrados / valores.Count);
    }
}