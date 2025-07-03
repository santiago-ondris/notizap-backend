using System.Text.RegularExpressions;
public static class InstagramContentAnalyzer
{
    /// <summary>
    /// Genera los top performers combinando todos los tipos de contenido
    /// </summary>
    public static List<TopContentItemDto> GenerarTopPerformers(
        List<InstagramReel> reels, 
        List<InstagramPost> posts, 
        List<InstagramStory> stories,
        int seguidores)
    {
        var topItems = new List<TopContentItemDto>();

        // Top 2 reels por views
        topItems.AddRange(reels
            .OrderByDescending(r => r.Views)
            .Take(2)
            .Select(r => new TopContentItemDto
            {
                Id = r.ReelId,
                TipoContenido = "reel",
                FechaPublicacion = r.FechaPublicacion,
                ImageUrl = r.ImageUrl,
                Url = r.Url,
                Contenido = TruncateContent(r.Contenido, 80),
                MetricaPrincipal = r.Views,
                NombreMetrica = "views",
                EngagementRate = InstagramAnalyticsHelper.CalcularEngagementRate(r.Interacciones, seguidores, r.Reach),
                MetricasAdicionales = new Dictionary<string, object>
                {
                    ["likes"] = r.Likes,
                    ["comentarios"] = r.Comentarios,
                    ["alcance"] = r.Reach,
                    ["guardados"] = r.Guardados
                }
            }));

        // Top 2 posts por likes
        topItems.AddRange(posts
            .OrderByDescending(p => p.Likes)
            .Take(2)
            .Select(p => new TopContentItemDto
            {
                Id = p.PostId,
                TipoContenido = "post",
                FechaPublicacion = p.FechaPublicacion,
                ImageUrl = p.ImageUrl,
                Url = p.Url,
                Contenido = TruncateContent(p.Content, 80),
                MetricaPrincipal = p.Likes,
                NombreMetrica = "likes",
                EngagementRate = InstagramAnalyticsHelper.CalcularEngagementRate((int)p.Interactions, seguidores, p.Reach),
                MetricasAdicionales = new Dictionary<string, object>
                {
                    ["comentarios"] = p.Comments,
                    ["compartidos"] = p.Shares,
                    ["alcance"] = p.Reach,
                    ["guardados"] = p.Saved
                }
            }));

        // Top 1 story por impressions
        topItems.AddRange(stories
            .OrderByDescending(s => s.Impressions)
            .Take(1)
            .Select(s => new TopContentItemDto
            {
                Id = s.PostId,
                TipoContenido = "story",
                FechaPublicacion = s.FechaPublicacion,
                ImageUrl = s.ThumbnailUrl,
                Url = s.Permalink,
                Contenido = TruncateContent(s.Content, 80),
                MetricaPrincipal = s.Impressions,
                NombreMetrica = "impressions",
                EngagementRate = InstagramAnalyticsHelper.CalcularEngagementRate(s.Replies, seguidores, s.Reach),
                MetricasAdicionales = new Dictionary<string, object>
                {
                    ["respuestas"] = s.Replies,
                    ["taps_adelante"] = s.TapsForward,
                    ["salidas"] = s.Exits
                }
            }));

        return topItems.OrderByDescending(t => t.MetricaPrincipal).ToList();
    }

    /// <summary>
    /// Analiza patrones de contenido y extrae insights
    /// </summary>
    public static PatronesContenidoDto AnalizarPatronesContenido(
        List<InstagramReel> reels, 
        List<InstagramPost> posts, 
        List<InstagramStory> stories, 
        int seguidores)
    {
        var analisisPorTipo = new List<TipoContenidoAnalisisDto>();

        // Análisis de Reels
        if (reels.Any())
        {
            var engagementReels = reels.Select(r => 
                InstagramAnalyticsHelper.CalcularEngagementRate(r.Interacciones, seguidores, r.Reach)).ToList();
            
            analisisPorTipo.Add(new TipoContenidoAnalisisDto
            {
                TipoContenido = "reel",
                EngagementPromedio = Math.Round(engagementReels.Average(), 2),
                TotalPublicaciones = reels.Count,
                AlcancePromedio = (int)reels.Average(r => (double)r.Reach),
                TemasRecurrentes = ExtractTemas(reels.Select(r => r.Contenido ?? "").ToList()),
                PorcentajeExito = InstagramMetricsCalculator.CalcularPorcentajeExito(engagementReels, 2.0)
            });
        }

        // Análisis de Posts
        if (posts.Any())
        {
            var engagementPosts = posts.Select(p => 
                InstagramAnalyticsHelper.CalcularEngagementRate((int)p.Interactions, seguidores, p.Reach)).ToList();
            
            analisisPorTipo.Add(new TipoContenidoAnalisisDto
            {
                TipoContenido = "post",
                EngagementPromedio = Math.Round(engagementPosts.Average(), 2),
                TotalPublicaciones = posts.Count,
                AlcancePromedio = (int)posts.Average(p => (double)p.Reach),
                TemasRecurrentes = ExtractTemas(posts.Select(p => p.Content ?? "").ToList()),
                PorcentajeExito = InstagramMetricsCalculator.CalcularPorcentajeExito(engagementPosts, 1.5)
            });
        }

        // Análisis de Stories
        if (stories.Any())
        {
            var engagementStories = stories.Select(s => 
                InstagramAnalyticsHelper.CalcularEngagementRate(s.Replies, seguidores, s.Reach)).ToList();
            
            analisisPorTipo.Add(new TipoContenidoAnalisisDto
            {
                TipoContenido = "story",
                EngagementPromedio = Math.Round(engagementStories.Average(), 2),
                TotalPublicaciones = stories.Count,
                AlcancePromedio = (int)stories.Average(s => (double)s.Reach),
                TemasRecurrentes = ExtractTemas(stories.Select(s => s.Content ?? "").ToList()),
                PorcentajeExito = InstagramMetricsCalculator.CalcularPorcentajeExito(engagementStories, 1.0)
            });
        }

        // Extraer palabras clave exitosas combinando todo el contenido
        var palabrasClaveExitosas = ExtractPalabrasClaveConEngagement(reels, posts, stories, seguidores);

        return new PatronesContenidoDto
        {
            AnalisisPorTipo = analisisPorTipo,
            PalabrasClaveExitosas = palabrasClaveExitosas,
            DuracionVideos = new DuracionVideoAnalisisDto
            {
                RangoDuracionOptimo = "15-30 segundos",
                EngagementPorDuracion = new Dictionary<string, double>(),
                DuracionPromedioExitosos = 20.0
            }
        };
    }

    /// <summary>
    /// Extrae temas recurrentes del contenido
    /// </summary>
    public static List<string> ExtractTemas(List<string> contenidos)
    {
        var palabrasComunes = contenidos
            .Where(c => !string.IsNullOrEmpty(c))
            .SelectMany(c => Regex.Matches(c.ToLower(), @"\b[a-záéíóúñü]{4,}\b")
                .Cast<Match>()
                .Select(m => m.Value))
            .Where(p => !EsPalabraComun(p))
            .GroupBy(p => p)
            .Where(g => g.Count() >= 2)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        return palabrasComunes;
    }

    /// <summary>
    /// Extrae palabras clave con su engagement asociado
    /// </summary>
    public static List<PalabraClaveDto> ExtractPalabrasClaveConEngagement(
        List<InstagramReel> reels,
        List<InstagramPost> posts, 
        List<InstagramStory> stories,
        int seguidores)
    {
        var palabrasContador = new Dictionary<string, (int frecuencia, List<double> engagements)>();

        // Procesar reels
        foreach (var reel in reels.Where(r => !string.IsNullOrEmpty(r.Contenido)))
        {
            var engagement = InstagramAnalyticsHelper.CalcularEngagementRate(reel.Interacciones, seguidores, reel.Reach);
            ProcesarContenidoPalabras(reel.Contenido, engagement, palabrasContador);
        }

        // Procesar posts
        foreach (var post in posts.Where(p => !string.IsNullOrEmpty(p.Content)))
        {
            var engagement = InstagramAnalyticsHelper.CalcularEngagementRate((int)post.Interactions, seguidores, post.Reach);
            ProcesarContenidoPalabras(post.Content!, engagement, palabrasContador);
        }

        // Procesar stories
        foreach (var story in stories.Where(s => !string.IsNullOrEmpty(s.Content)))
        {
            var engagement = InstagramAnalyticsHelper.CalcularEngagementRate(story.Replies, seguidores, story.Reach);
            ProcesarContenidoPalabras(story.Content!, engagement, palabrasContador);
        }

        var promedioGeneral = palabrasContador.Values
            .SelectMany(v => v.engagements)
            .DefaultIfEmpty(0)
            .Average();

        return palabrasContador
            .Where(p => p.Value.frecuencia >= 2)
            .Select(p => new PalabraClaveDto
            {
                Palabra = p.Key,
                Frecuencia = p.Value.frecuencia,
                EngagementPromedio = Math.Round(p.Value.engagements.Average(), 2),
                ImpactoPositivo = CalcularImpactoPositivo(p.Value.engagements, promedioGeneral)
            })
            .OrderByDescending(p => p.ImpactoPositivo)
            .Take(10)
            .ToList();
    }

    // Métodos auxiliares privados
    private static string TruncateContent(string? content, int maxLength)
    {
        if (string.IsNullOrEmpty(content)) return "";
        return content.Length > maxLength ? content[..maxLength] + "..." : content;
    }

    private static void ProcesarContenidoPalabras(string contenido, double engagement, 
        Dictionary<string, (int frecuencia, List<double> engagements)> palabrasContador)
    {
        var palabras = Regex.Matches(contenido.ToLower(), @"\b[a-záéíóúñü]{3,}\b")
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

    private static bool EsPalabraComun(string palabra)
    {
        var palabrasComunes = new HashSet<string> 
        { 
            "que", "con", "para", "por", "una", "los", "las", "del", "como", "más", 
            "muy", "todo", "sobre", "cuando", "donde", "desde", "hasta", "entre",
            "esta", "este", "pero", "también", "solo", "bien", "aquí", "ahora"
        };
        return palabrasComunes.Contains(palabra);
    }

    private static double CalcularImpactoPositivo(List<double> engagementsConPalabra, double promedioGeneral)
    {
        if (promedioGeneral <= 0) return 0;
        var promedioConPalabra = engagementsConPalabra.Average();
        return Math.Round(((promedioConPalabra - promedioGeneral) / promedioGeneral) * 100, 2);
    }
}