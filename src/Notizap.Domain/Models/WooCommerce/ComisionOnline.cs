using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ComisionOnline
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int Mes { get; set; }

    [Required]
    public int Año { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSinNC { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoAndreani { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoOCA { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoCaddy { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    // Propiedades calculadas
    [NotMapped]
    public decimal TotalEnvios => MontoAndreani + MontoOCA + MontoCaddy;

    [NotMapped]
    public decimal BaseCalculo => TotalSinNC - TotalEnvios;

    [NotMapped]
    public decimal BaseCalculoSinIVA => BaseCalculo * 0.79m; // Se resta el 21%

    [NotMapped]
    public decimal ComisionBruta => BaseCalculoSinIVA * 0.005m;

    [NotMapped]
    public decimal ComisionPorPersona => ComisionBruta / 6m;

    [NotMapped]
    public string PeriodoCompleto => $"{ObtenerNombreMes(Mes)} {Año}";

    private static string ObtenerNombreMes(int mes)
    {
        return mes switch
        {
            1 => "Enero",
            2 => "Febrero", 
            3 => "Marzo",
            4 => "Abril",
            5 => "Mayo",
            6 => "Junio",
            7 => "Julio",
            8 => "Agosto",
            9 => "Septiembre",
            10 => "Octubre",
            11 => "Noviembre",
            12 => "Diciembre",
            _ => "Mes Inválido"
        };
    }
}