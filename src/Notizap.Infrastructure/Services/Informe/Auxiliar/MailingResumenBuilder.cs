public class MailingResumenBuilder
{
    private readonly IMailchimpQueryService _queryService;

    public MailingResumenBuilder(IMailchimpQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<MailingResumenGlobalDto> ConstruirAsync(int year, int month)
    {
        var montella = await ConstruirParaCuenta("Montella", year, month);
        var alenka = await ConstruirParaCuenta("Alenka", year, month);

        return new MailingResumenGlobalDto
        {
            Montella = montella,
            Alenka = alenka
        };
    }

    private async Task<MailingResumenDto> ConstruirParaCuenta(string cuenta, int year, int month)
    {
        var highlights = await _queryService.GetHighlightsAsync(cuenta);
        var campañas = await _queryService.GetAllCampaignsAsync(cuenta);

        var totalEnviados = campañas
            .Where(c => c.SendTime.Year == year && c.SendTime.Month == month)
            .Sum(c => c.EmailsSent);

        return new MailingResumenDto
        {
            MejorOpenRateCampania = highlights.BestOpenRateCampaign,
            OpenRate = highlights.BestOpenRate,
            MejorClickRateCampania = highlights.BestClickRateCampaign,
            ClickRate = highlights.BestClickRate,
            MejorConversionCampania = highlights.BestConversionCampaign,
            Conversiones = highlights.BestConversions,
            TotalMailsEnviados = totalEnviados
        };
    }
}
