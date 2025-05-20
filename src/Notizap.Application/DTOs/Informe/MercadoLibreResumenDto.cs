public class MercadoLibreResumenDto
{
    public decimal Total { get; set; }
    public int Unidades { get; set; }
    public List<string>? TopProductos { get; set; }

    public decimal InversionTotal { get; set; }
    public decimal InversionProductAds { get; set; }
    public decimal InversionBrandAds { get; set; }
    public decimal InversionDisplayAds { get; set; }

    public int CampañasProductAds { get; set; }
    public int CampañasBrandAds { get; set; }
    public int CampañasDisplayAds { get; set; }
}