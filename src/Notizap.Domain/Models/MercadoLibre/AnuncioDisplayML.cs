public class AnuncioDisplayML
{
    public int Id { get; set; }

    public int ReportePublicidadMLId { get; set; }
    public ReportePublicidadML Reporte { get; set; } = default!;

    public string Nombre { get; set; } = default!;
    public int Impresiones { get; set; }
    public int Clics { get; set; }
    public int Visitas { get; set; }
    public decimal Ctr { get; set; }
}