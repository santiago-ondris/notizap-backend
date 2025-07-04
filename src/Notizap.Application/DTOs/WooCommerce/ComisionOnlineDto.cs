using System.ComponentModel.DataAnnotations;
public class ComisionOnlineDto
{
    public int Id { get; set; }
    public int Mes { get; set; }
    public int Año { get; set; }
    public decimal TotalSinNC { get; set; }
    public decimal MontoAndreani { get; set; }
    public decimal MontoOCA { get; set; }
    public decimal MontoCaddy { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    
    // Campos calculados
    public decimal TotalEnvios { get; set; }
    public decimal BaseCalculo { get; set; }
    public decimal BaseCalculoSinIVA { get; set; }
    public decimal ComisionBruta { get; set; }
    public decimal ComisionPorPersona { get; set; }
    public string PeriodoCompleto { get; set; } = string.Empty;
}

public class CreateComisionOnlineDto
{
    [Required(ErrorMessage = "El mes es requerido")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12")]
    public int Mes { get; set; }

    [Required(ErrorMessage = "El año es requerido")]
    [Range(2020, 2030, ErrorMessage = "El año debe estar entre 2020 y 2030")]
    public int Año { get; set; }

    [Required(ErrorMessage = "El total sin NC es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El total sin NC debe ser mayor o igual a 0")]
    public decimal TotalSinNC { get; set; }

    [Required(ErrorMessage = "El monto de Andreani es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El monto de Andreani debe ser mayor o igual a 0")]
    public decimal MontoAndreani { get; set; }

    [Required(ErrorMessage = "El monto de OCA es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El monto de OCA debe ser mayor o igual a 0")]
    public decimal MontoOCA { get; set; }

    [Required(ErrorMessage = "El monto de Caddy es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El monto de Caddy debe ser mayor o igual a 0")]
    public decimal MontoCaddy { get; set; }
}

public class UpdateComisionOnlineDto : CreateComisionOnlineDto
{
    [Required(ErrorMessage = "El ID es requerido")]
    public int Id { get; set; }
}

public class ComisionOnlineQueryDto
{
    public int? Mes { get; set; }
    public int? Año { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    
    // Paginación
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // Ordenamiento
    public string OrderBy { get; set; } = "FechaCreacion";
    public bool OrderDescending { get; set; } = true;
}

public class CalculoComisionDto
{
    public decimal TotalSinNC { get; set; }
    public decimal TotalEnvios { get; set; }
    public decimal BaseCalculo { get; set; }
    public decimal BaseCalculoSinIVA { get; set; }
    public decimal ComisionBruta { get; set; }
    public decimal ComisionPorPersona { get; set; }
    public List<DetalleEnvioDto> DetalleEnvios { get; set; } = new();
}

public class DetalleEnvioDto
{
    public string Empresa { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}