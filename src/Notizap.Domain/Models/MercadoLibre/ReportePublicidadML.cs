
public class ReportePublicidadML
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public TipoPublicidadML Tipo { get; set; }

    public string NombreCampania { get; set; } = default!;

    // Campos específicos Product Ads
    public decimal? AcosObjetivo { get; set; }
    public int? VentasPads { get; set; }
    public decimal? AcosReal { get; set; }

    // Campos específicos Brand Ads
    public decimal? Presupuesto { get; set; }
    public int? Ventas { get; set; }
    public decimal? Cpc { get; set; }

    // Comunes a varios tipos
    public int? Impresiones { get; set; }
    public int? Clics { get; set; }
    public decimal? Ingresos { get; set; }
    public decimal Inversion { get; set; }

    // Campos Display Ads
    public int? VisitasConDisplay { get; set; }
    public int? VisitasSinDisplay { get; set; }
    public int? Alcance { get; set; }
    public decimal? CostoPorVisita { get; set; }

    public List<AnuncioDisplayML> Anuncios { get; set; } = new();
}