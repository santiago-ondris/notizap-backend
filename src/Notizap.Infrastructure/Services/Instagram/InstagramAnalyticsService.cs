using Microsoft.EntityFrameworkCore;

namespace Notizap.Infrastructure.Services
{
    public class InstagramAnalyticsService : IInstagramAnalyticsService
    {
        private readonly NotizapDbContext _context;

        public InstagramAnalyticsService(NotizapDbContext context)
        {
            _context = context;
        }

        public async Task<InstagramDashboardDto> GetDashboardAsync(string cuenta, DateTime? desde = null, DateTime? hasta = null)
        {
            // Fechas por defecto: últimos 30 días
            var fechaHasta = hasta ?? DateTime.UtcNow;
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            // Obtener datos básicos
            var seguidoresActuales = await GetSeguidoresActuales(cuenta);
            var seguidoresAnterior = await GetSeguidoresFecha(cuenta, fechaDesde.AddDays(-7));

            // Obtener contenido del período
            var reels = await GetReelsPeriodo(cuenta, fechaDesde, fechaHasta);
            var posts = await GetPostsPeriodo(cuenta, fechaDesde, fechaHasta);
            var stories = await GetStoriesPeriodo(cuenta, fechaDesde, fechaHasta);

            // Calcular métricas usando helpers
            var metricas = InstagramMetricsCalculator.CalcularMetricasGenerales(
                seguidoresActuales, seguidoresAnterior, reels, posts, stories);

            var evolucionSeguidores = await GetEvolucionSeguidoresDetallada(cuenta, fechaDesde, fechaHasta);

            var comparativa = InstagramMetricsCalculator.CalcularComparativaContenido(
                reels, posts, stories, seguidoresActuales);

            var topPerformers = InstagramContentAnalyzer.GenerarTopPerformers(
                reels, posts, stories, seguidoresActuales);

            return new InstagramDashboardDto
            {
                Cuenta = cuenta,
                MetricasGenerales = metricas,
                EvolucionSeguidores = evolucionSeguidores,
                ComparativaContenido = comparativa,
                TopPerformers = topPerformers,
                UltimaSincronizacion = await GetUltimaSincronizacion(cuenta)
            };
        }

        public async Task<AnalisisPatronesDto> GetAnalisisPatronesAsync(string cuenta, DateTime desde, DateTime hasta)
        {
            // Obtener datos del período
            var reels = await GetReelsPeriodo(cuenta, desde, hasta);
            var posts = await GetPostsPeriodo(cuenta, desde, hasta);
            var stories = await GetStoriesPeriodo(cuenta, desde, hasta);
            var seguidoresPromedio = await GetSeguidoresPromedio(cuenta, desde, hasta);

            // Análisis usando helpers
            var patronesTempo = InstagramTemporalAnalyzer.AnalizarPatronesTemporales(
                reels, posts, stories, seguidoresPromedio);

            var patronesContenido = InstagramContentAnalyzer.AnalizarPatronesContenido(
                reels, posts, stories, seguidoresPromedio);

            var totalPublicaciones = reels.Count + posts.Count + stories.Count;

            // Generar recomendaciones
            var recomendacionesTempo = InstagramTemporalAnalyzer.GenerarRecomendacionesTempo(
                patronesTempo.MejoresHorarios, patronesTempo.MejoresDias, totalPublicaciones);

            return new AnalisisPatronesDto
            {
                Cuenta = cuenta,
                PeriodoDesde = desde,
                PeriodoHasta = hasta,
                PatronesTemporales = patronesTempo,
                PatronesContenido = patronesContenido,
                Recomendaciones = new RecomendacionesDto
                {
                    HorariosRecomendados = patronesTempo.MejoresHorarios.Take(3).Select(h => h.RangoHorario).ToList(),
                    DiasRecomendados = patronesTempo.MejoresDias.Take(2).Select(d => d.DiaSemana).ToList(),
                    TiposContenidoRecomendados = recomendacionesTempo,
                    ConfianzaRecomendacion = CalcularConfianzaRecomendacion(totalPublicaciones)
                }
            };
        }

        public async Task<List<EvolucionSeguidoresDto>> GetEvolucionSeguidoresDetallada(string cuenta, DateTime desde, DateTime hasta)
        {
            var seguidoresPorDia = await _context.InstagramSeguidores
                .Where(s => s.Cuenta == cuenta && s.Date >= desde && s.Date <= hasta)
                .OrderBy(s => s.Date)
                .Select(s => new { s.Date, s.Value })
                .ToListAsync();

            var datosParaProcesar = seguidoresPorDia.Select(s => (s.Date, s.Value)).ToList();
            
            return InstagramTemporalAnalyzer.ProcesarEvolucionSeguidores(datosParaProcesar);
        }

        public async Task<Dictionary<string, double>> GetEngagementRatePorTipoAsync(string cuenta, DateTime desde, DateTime hasta)
        {
            var seguidores = await GetSeguidoresPromedio(cuenta, desde, hasta);
            var reels = await GetReelsPeriodo(cuenta, desde, hasta);
            var posts = await GetPostsPeriodo(cuenta, desde, hasta);
            var stories = await GetStoriesPeriodo(cuenta, desde, hasta);

            return InstagramMetricsCalculator.CalcularEngagementRatePorTipo(reels, posts, stories, seguidores);
        }

        public async Task<List<HorarioPerformanceDto>> GetMejoresHorariosAsync(string cuenta, int diasAnalisis = 30)
        {
            var fecha = DateTime.UtcNow.AddDays(-diasAnalisis);
            var seguidores = await GetSeguidoresPromedio(cuenta, fecha, DateTime.UtcNow);

            var reels = await GetReelsPeriodo(cuenta, fecha, DateTime.UtcNow);
            var posts = await GetPostsPeriodo(cuenta, fecha, DateTime.UtcNow);

            return InstagramTemporalAnalyzer.AnalizarMejoresHorarios(reels, posts, seguidores, diasAnalisis);
        }

        public async Task<ResumenEjecutivoDto> GetResumenEjecutivoAsync(string cuenta, DateTime desde, DateTime hasta)
        {
            var dashboard = await GetDashboardAsync(cuenta, desde, hasta);
            var analisisPatrones = await GetAnalisisPatronesAsync(cuenta, desde, hasta);
            
            var tendencia = InstagramAnalyticsHelper.DeterminarTendencia(
                dashboard.EvolucionSeguidores.Select(e => e.Seguidores).ToList());

            var insights = InstagramTemporalAnalyzer.GenerarInsightsTempo(
                analisisPatrones.PatronesTemporales.MejoresHorarios,
                analisisPatrones.PatronesTemporales.MejoresDias,
                dashboard.MetricasGenerales.EngagementPromedio);

            return new ResumenEjecutivoDto
            {
                Cuenta = cuenta,
                PeriodoAnalizado = hasta,
                TendenciaGeneral = tendencia,
                LogrosPrincipales = GenerarLogros(dashboard.MetricasGenerales),
                AreasDeOportunidad = insights.Take(2).ToList(),
                AccionesRecomendadas = analisisPatrones.Recomendaciones.TiposContenidoRecomendados.Take(3).ToList(),
                MetricasClave = new Dictionary<string, string>
                {
                    ["Engagement"] = $"{dashboard.MetricasGenerales.EngagementPromedio:F1}%",
                    ["Crecimiento"] = $"{dashboard.MetricasGenerales.PorcentajeCrecimientoSemanal:F1}%",
                    ["Publicaciones"] = dashboard.MetricasGenerales.TotalPublicacionesPeriodo.ToString(),
                    ["Alcance Promedio"] = dashboard.MetricasGenerales.AlcancePromedio.ToString("N0")
                }
            };
        }

        public async Task<ComparativaPeriodosDto> GetComparativaPeriodosAsync(
            string cuenta, 
            DateTime periodoActualDesde, DateTime periodoActualHasta, 
            DateTime periodoAnteriorDesde, DateTime periodoAnteriorHasta)
        {
            var dashboardActual = await GetDashboardAsync(cuenta, periodoActualDesde, periodoActualHasta);
            var dashboardAnterior = await GetDashboardAsync(cuenta, periodoAnteriorDesde, periodoAnteriorHasta);

            var cambioSeguidores = dashboardActual.MetricasGenerales.SeguidoresActuales - dashboardAnterior.MetricasGenerales.SeguidoresActuales;
            var cambioEngagement = dashboardActual.MetricasGenerales.EngagementPromedio - dashboardAnterior.MetricasGenerales.EngagementPromedio;
            var cambioPublicaciones = dashboardActual.MetricasGenerales.TotalPublicacionesPeriodo - dashboardAnterior.MetricasGenerales.TotalPublicacionesPeriodo;

            var cambios = new CambiosPeriodoDto
            {
                CambioSeguidores = cambioSeguidores,
                PorcentajeCambioSeguidores = InstagramAnalyticsHelper.CalcularCrecimientoPorcentual(
                    dashboardActual.MetricasGenerales.SeguidoresActuales, 
                    dashboardAnterior.MetricasGenerales.SeguidoresActuales),
                CambioEngagement = cambioEngagement,
                PorcentajeCambioEngagement = dashboardAnterior.MetricasGenerales.EngagementPromedio > 0 ? 
                    Math.Round((cambioEngagement / dashboardAnterior.MetricasGenerales.EngagementPromedio) * 100, 2) : 0,
                CambioPublicaciones = cambioPublicaciones,
                PorcentajeCambioPublicaciones = InstagramAnalyticsHelper.CalcularCrecimientoPorcentual(
                    dashboardActual.MetricasGenerales.TotalPublicacionesPeriodo,
                    dashboardAnterior.MetricasGenerales.TotalPublicacionesPeriodo),
                TendenciaGeneral = DeterminarTendenciaComparativa(cambioEngagement, cambioSeguidores),
                InsightsClaves = GenerarInsightsComparativos(cambioEngagement, cambioSeguidores, cambioPublicaciones)
            };

            return new ComparativaPeriodosDto
            {
                Cuenta = cuenta,
                PeriodoActual = new PeriodoMetricasDto
                {
                    Desde = periodoActualDesde,
                    Hasta = periodoActualHasta,
                    Seguidores = dashboardActual.MetricasGenerales.SeguidoresActuales,
                    EngagementPromedio = dashboardActual.MetricasGenerales.EngagementPromedio,
                    TotalPublicaciones = dashboardActual.MetricasGenerales.TotalPublicacionesPeriodo,
                    TotalInteracciones = dashboardActual.MetricasGenerales.TotalInteracciones,
                    AlcancePromedio = dashboardActual.MetricasGenerales.AlcancePromedio
                },
                PeriodoAnterior = new PeriodoMetricasDto
                {
                    Desde = periodoAnteriorDesde,
                    Hasta = periodoAnteriorHasta,
                    Seguidores = dashboardAnterior.MetricasGenerales.SeguidoresActuales,
                    EngagementPromedio = dashboardAnterior.MetricasGenerales.EngagementPromedio,
                    TotalPublicaciones = dashboardAnterior.MetricasGenerales.TotalPublicacionesPeriodo,
                    TotalInteracciones = dashboardAnterior.MetricasGenerales.TotalInteracciones,
                    AlcancePromedio = dashboardAnterior.MetricasGenerales.AlcancePromedio
                },
                Cambios = cambios
            };
        }

        // Métodos auxiliares privados para obtener datos
        private async Task<List<InstagramReel>> GetReelsPeriodo(string cuenta, DateTime desde, DateTime hasta)
        {
            return await _context.InstagramReels
                .Where(r => r.Cuenta == cuenta && r.FechaPublicacion >= desde && r.FechaPublicacion <= hasta)
                .ToListAsync();
        }

        private async Task<List<InstagramPost>> GetPostsPeriodo(string cuenta, DateTime desde, DateTime hasta)
        {
            return await _context.InstagramPosts
                .Where(p => p.Cuenta == cuenta && p.FechaPublicacion >= desde && p.FechaPublicacion <= hasta)
                .ToListAsync();
        }

        private async Task<List<InstagramStory>> GetStoriesPeriodo(string cuenta, DateTime desde, DateTime hasta)
        {
            return await _context.InstagramStories
                .Where(s => s.Cuenta == cuenta && s.FechaPublicacion >= desde && s.FechaPublicacion <= hasta)
                .ToListAsync();
        }

        private async Task<int> GetSeguidoresActuales(string cuenta)
        {
            var ultimo = await _context.InstagramSeguidores
                .Where(s => s.Cuenta == cuenta)
                .OrderByDescending(s => s.Date)
                .FirstOrDefaultAsync();
            
            return ultimo?.Value ?? 0;
        }

        private async Task<int> GetSeguidoresFecha(string cuenta, DateTime fecha)
        {
            var seguidores = await _context.InstagramSeguidores
                .Where(s => s.Cuenta == cuenta && s.Date <= fecha)
                .OrderByDescending(s => s.Date)
                .FirstOrDefaultAsync();
            
            return seguidores?.Value ?? 0;
        }

        private async Task<int> GetSeguidoresPromedio(string cuenta, DateTime desde, DateTime hasta)
        {
            var promedio = await _context.InstagramSeguidores
                .Where(s => s.Cuenta == cuenta && s.Date >= desde && s.Date <= hasta)
                .AverageAsync(s => (double?)s.Value);
            
            return (int)(promedio ?? 0);
        }

        private async Task<DateTime> GetUltimaSincronizacion(string cuenta)
        {
            var ultimoReel = await _context.InstagramReels
                .Where(r => r.Cuenta == cuenta)
                .OrderByDescending(r => r.FechaPublicacion)
                .Select(r => r.FechaPublicacion)
                .FirstOrDefaultAsync();

            var ultimoPost = await _context.InstagramPosts
                .Where(p => p.Cuenta == cuenta)
                .OrderByDescending(p => p.FechaPublicacion)
                .Select(p => p.FechaPublicacion)
                .FirstOrDefaultAsync();

            return new[] { ultimoReel, ultimoPost }.Max();
        }

        // Métodos auxiliares para lógica de negocio
        private double CalcularConfianzaRecomendacion(int totalPublicaciones)
        {
            return totalPublicaciones switch
            {
                >= 50 => 0.9,
                >= 30 => 0.8,
                >= 20 => 0.7,
                >= 10 => 0.6,
                _ => 0.4
            };
        }

        private List<string> GenerarLogros(MetricasGeneralesDto metricas)
        {
            var logros = new List<string>();

            if (metricas.PorcentajeCrecimientoSemanal > 2)
                logros.Add($"Crecimiento de seguidores del {metricas.PorcentajeCrecimientoSemanal:F1}%");

            if (metricas.EngagementPromedio > 2.5)
                logros.Add($"Engagement superior al promedio ({metricas.EngagementPromedio:F1}%)");

            if (metricas.TotalPublicacionesPeriodo > 20)
                logros.Add("Consistencia en la frecuencia de publicación");

            return logros.Any() ? logros : new List<string> { "Mantenimiento de presencia activa en redes" };
        }

        private string DeterminarTendenciaComparativa(double cambioEngagement, int cambioSeguidores)
        {
            if (cambioEngagement > 0.5 && cambioSeguidores > 0)
                return "Mejorando";
            else if (cambioEngagement < -0.5 || cambioSeguidores < 0)
                return "Declinando";
            else
                return "Estable";
        }

        private List<string> GenerarInsightsComparativos(double cambioEngagement, int cambioSeguidores, int cambioPublicaciones)
        {
            var insights = new List<string>();

            if (Math.Abs(cambioEngagement) > 0.5)
            {
                var direccion = cambioEngagement > 0 ? "mejoró" : "disminuyó";
                insights.Add($"El engagement {direccion} {Math.Abs(cambioEngagement):F1}% respecto al período anterior");
            }

            if (Math.Abs(cambioSeguidores) > 50)
            {
                var direccion = cambioSeguidores > 0 ? "ganó" : "perdió";
                insights.Add($"La cuenta {direccion} {Math.Abs(cambioSeguidores)} seguidores");
            }

            if (cambioPublicaciones != 0)
            {
                var direccion = cambioPublicaciones > 0 ? "aumentó" : "disminuyó";
                insights.Add($"La frecuencia de publicación {direccion} en {Math.Abs(cambioPublicaciones)} posts");
            }

            return insights;
        }
    }
}