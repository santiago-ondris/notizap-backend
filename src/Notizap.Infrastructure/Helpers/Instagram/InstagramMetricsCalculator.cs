public static class InstagramMetricsCalculator
{
    /// <summary>
    /// Calcula las métricas generales del dashboard
    /// </summary>
    public static MetricasGeneralesDto CalcularMetricasGenerales(
        int seguidoresActuales, 
        int seguidoresAnterior,
        List<InstagramReel> reels, 
        List<InstagramPost> posts, 
        List<InstagramStory> stories)
    {
        var totalPublicaciones = reels.Count + posts.Count + stories.Count;
        var totalInteracciones = reels.Sum(r => r.Interacciones) + 
                                posts.Sum(p => (int)p.Interactions) + 
                                stories.Sum(s => s.Replies + s.TapsForward);

        var engagementReels = reels.Any() ? 
            reels.Average(r => InstagramAnalyticsHelper.CalcularEngagementRate(r.Interacciones, seguidoresActuales, r.Reach)) : 0;
        var engagementPosts = posts.Any() ? 
            posts.Average(p => InstagramAnalyticsHelper.CalcularEngagementRate((int)p.Interactions, seguidoresActuales, p.Reach)) : 0;
        var engagementStories = stories.Any() ? 
            stories.Average(s => InstagramAnalyticsHelper.CalcularEngagementRate(s.Replies, seguidoresActuales, s.Reach)) : 0;

        var engagementPromedio = new[] { engagementReels, engagementPosts, engagementStories }
            .Where(e => e > 0).DefaultIfEmpty(0).Average();

        var alcancePromedio = (int)new[] { 
            reels.Any() ? reels.Average(r => (double)r.Reach) : 0,
            posts.Any() ? posts.Average(p => (double)p.Reach) : 0,
            stories.Any() ? stories.Average(s => (double)s.Reach) : 0 
        }.Where(a => a > 0).DefaultIfEmpty(0).Average();

        return new MetricasGeneralesDto
        {
            SeguidoresActuales = seguidoresActuales,
            CrecimientoSemanal = seguidoresActuales - seguidoresAnterior,
            PorcentajeCrecimientoSemanal = InstagramAnalyticsHelper.CalcularCrecimientoPorcentual(seguidoresActuales, seguidoresAnterior),
            TotalPublicacionesPeriodo = totalPublicaciones,
            EngagementPromedio = Math.Round(engagementPromedio, 2),
            TotalInteracciones = totalInteracciones,
            AlcancePromedio = alcancePromedio
        };
    }

    /// <summary>
    /// Calcula la comparativa entre tipos de contenido
    /// </summary>
    public static ComparativaContenidoDto CalcularComparativaContenido(
        List<InstagramReel> reels, 
        List<InstagramPost> posts, 
        List<InstagramStory> stories, 
        int seguidores)
    {
        return new ComparativaContenidoDto
        {
            Reels = CalcularMetricasTipoContenido(
                reels,
                r => r.Interacciones,
                r => r.Reach,
                r => r.Views,
                seguidores,
                "views"
            ),
            Posts = CalcularMetricasTipoContenido(
                posts,
                p => (int)p.Interactions,
                p => p.Reach,
                p => p.Likes,
                seguidores,
                "likes"
            ),
            Stories = CalcularMetricasTipoContenido(
                stories,
                s => s.Replies + s.TapsForward,
                s => s.Reach,
                s => s.Impressions,
                seguidores,
                "impressions"
            )
        };
    }

    /// <summary>
    /// Calcula métricas para un tipo específico de contenido
    /// </summary>
    public static ContenidoMetricasDto CalcularMetricasTipoContenido<T>(
        List<T> contenidos,
        Func<T, int> getInteracciones,
        Func<T, int> getAlcance,
        Func<T, int> getMetricaPrincipal,
        int seguidores,
        string tipoMetrica)
    {
        if (!contenidos.Any())
        {
            return new ContenidoMetricasDto();
        }

        var engagements = contenidos.Select(c => 
            InstagramAnalyticsHelper.CalcularEngagementRate(getInteracciones(c), seguidores, getAlcance(c))
        ).ToList();

        var mejorItem = contenidos.OrderByDescending(getMetricaPrincipal).First();

        return new ContenidoMetricasDto
        {
            TotalPublicaciones = contenidos.Count,
            EngagementPromedio = Math.Round(engagements.Average(), 2),
            AlcancePromedio = (int)contenidos.Average(c => (double)getAlcance(c)),
            InteraccionesPromedio = (int)contenidos.Average(c => (double)getInteracciones(c)),
            MejorPerformance = getMetricaPrincipal(mejorItem),
            TipoMetricaMejor = tipoMetrica
        };
    }

    /// <summary>
    /// Calcula el engagement rate por tipo de contenido
    /// </summary>
    public static Dictionary<string, double> CalcularEngagementRatePorTipo(
        List<InstagramReel> reels,
        List<InstagramPost> posts, 
        List<InstagramStory> stories,
        int seguidores)
    {
        var resultado = new Dictionary<string, double>();

        if (reels.Any())
        {
            resultado["reels"] = Math.Round(reels.Average(r => 
                InstagramAnalyticsHelper.CalcularEngagementRate(r.Interacciones, seguidores, r.Reach)), 2);
        }

        if (posts.Any())
        {
            resultado["posts"] = Math.Round(posts.Average(p => 
                InstagramAnalyticsHelper.CalcularEngagementRate((int)p.Interactions, seguidores, p.Reach)), 2);
        }

        if (stories.Any())
        {
            resultado["stories"] = Math.Round(stories.Average(s => 
                InstagramAnalyticsHelper.CalcularEngagementRate(s.Replies, seguidores, s.Reach)), 2);
        }

        return resultado;
    }

    /// <summary>
    /// Calcula el porcentaje de éxito basado en una baseline
    /// </summary>
    public static double CalcularPorcentajeExito(List<double> engagements, double baseline)
    {
        if (!engagements.Any()) return 0;
        var exitosos = engagements.Count(e => e >= baseline);
        return Math.Round((double)exitosos / engagements.Count * 100, 1);
    }
}