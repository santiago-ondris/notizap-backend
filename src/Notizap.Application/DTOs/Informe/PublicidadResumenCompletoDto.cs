public class PublicidadResumenCompletoDto
{
    public decimal InversionTotal { get; set; }

    // Segmentado
    public decimal InversionMetaMontella { get; set; }
    public decimal InversionMetaAlenka { get; set; }
    public decimal InversionMetaKids { get; set; }
    public decimal InversionGoogleMontella { get; set; }

    public List<AdCampaignResumenDto> CampañasMeta { get; set; } = new();
    public List<AdCampaignResumenDto> CampañasGoogle { get; set; } = new();
}