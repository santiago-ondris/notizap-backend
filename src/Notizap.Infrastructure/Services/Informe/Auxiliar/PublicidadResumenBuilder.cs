public class PublicidadResumenBuilder
{
    private readonly IAdService _adService;

    public PublicidadResumenBuilder(IAdService adService)
    {
        _adService = adService;
    }

    public async Task<PublicidadResumenCompletoDto> ConstruirAsync(int year, int month)
    {
        var reportes = await _adService.GetAllAsync();
        var delMes = reportes.Where(r => r.Year == year && r.Month == month).ToList();

        var campanas = delMes.SelectMany(r => r.Campañas.Select(c => new AdCampaignResumenDto
        {
            Nombre = c.Nombre,
            Plataforma = r.Plataforma,
            UnidadNegocio = r.UnidadNegocio,
            Inversion = c.MontoInvertido,
            Seguidores = c.FollowersCount,
            Resultados = c.Resultados 
        })).ToList();

        decimal totalMetaMontella = campanas.Where(c => c.Plataforma == "Meta" && c.UnidadNegocio == "Montella").Sum(c => c.Inversion);
        decimal totalMetaAlenka = campanas.Where(c => c.Plataforma == "Meta" && c.UnidadNegocio == "Alenka").Sum(c => c.Inversion);
        decimal totalMetaKids = campanas.Where(c => c.Plataforma == "Meta" && c.UnidadNegocio == "Kids").Sum(c => c.Inversion);
        decimal totalGoogleMontella = campanas.Where(c => c.Plataforma == "Google" && c.UnidadNegocio == "Montella").Sum(c => c.Inversion);

        var resumen = new PublicidadResumenCompletoDto
        {
            InversionTotal = campanas.Sum(c => c.Inversion),

            InversionMetaMontella = totalMetaMontella,
            InversionMetaAlenka = totalMetaAlenka,
            InversionMetaKids = totalMetaKids,
            InversionGoogleMontella = totalGoogleMontella,

            CampañasMeta = campanas.Where(c => c.Plataforma == "Meta").ToList(),
            CampañasGoogle = campanas.Where(c => c.Plataforma == "Google").ToList()
        };

        return resumen;
    }
}
