public class InstagramResumenBuilder
{
    private readonly IReelsService _reelsService;
    private readonly IFollowersService _followersService;
    private readonly IPostsService _postsService;
    private readonly IStoriesService _storiesService;

    public InstagramResumenBuilder(
        IReelsService reelsService,
        IFollowersService followersService,
        IPostsService postsService,
        IStoriesService storiesService)
    {
        _reelsService = reelsService;
        _followersService = followersService;
        _postsService = postsService;
        _storiesService = storiesService;
    }

    public async Task<InstagramResumenDto> ConstruirAsync(int year, int month)
    {
        var from = new DateTime(year, month, 1);
        var to = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        return new InstagramResumenDto
        {
            CuentaMontella = await ResumenPorCuenta("Montella", from, to),
            CuentaAlenka = await ResumenPorCuenta("Alenka", from, to),
            CuentaKids = await ResumenPorCuenta("Kids", from, to)
        };
    }

    private async Task<InstagramCuentaResumenDto> ResumenPorCuenta(string cuenta, DateTime from, DateTime to)
    {
        var seguidores = await _followersService.GetFollowersMetricsAsync(cuenta, from, to);
        var nuevosSeguidores = (seguidores.LastOrDefault()?.Value ?? 0) - (seguidores.FirstOrDefault()?.Value ?? 0);

        var reels = await _reelsService.GetAllReelsAsync(cuenta, from, to);

        var reelMasVisto = (await _reelsService.GetTopReelsByViewsAsync(cuenta, from, to, 1)).FirstOrDefault();
        var reelMasLikes = (await _reelsService.GetTopReelsByLikesAsync(cuenta, from, to, 1)).FirstOrDefault();

        var posteos = await _postsService.GetTopPostsAsync(cuenta, from, to, "likes", 1000);
        var historias = await _storiesService.GetTopStoriesAsync(cuenta, from, to, "impressions", 1000);

        return new InstagramCuentaResumenDto
        {
            MasVisto = reelMasVisto == null ? null : new ReelTopResumen
            {
                FechaPublicacion = reelMasVisto.FechaPublicacion,
                Views = reelMasVisto.Views,
                Likes = reelMasVisto.Likes
            },
            MasLikeado = reelMasLikes == null ? null : new ReelTopResumen
            {
                FechaPublicacion = reelMasLikes.FechaPublicacion,
                Views = reelMasLikes.Views,
                Likes = reelMasLikes.Likes
            },
            CantidadReels = reels.Count,
            CantidadPosteos = posteos.Count,
            CantidadHistorias = historias.Count,
            NuevosSeguidores = nuevosSeguidores
        };
    }
}
