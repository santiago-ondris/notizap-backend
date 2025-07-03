using System.ComponentModel.DataAnnotations;
public class VentaWooCommerceDto
{
    public int Id { get; set; }
    public string Tienda { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Año { get; set; }
    public decimal MontoFacturado { get; set; }
    public int UnidadesVendidas { get; set; }
    public List<string> TopProductos { get; set; } = new();
    public List<string> TopCategorias { get; set; } = new();
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string PeriodoCompleto { get; set; } = string.Empty;
}

// DTO para crear nueva venta (Request)
public class CreateVentaWooCommerceDto
{
    [Required(ErrorMessage = "La tienda es requerida")]
    [StringLength(100, ErrorMessage = "El nombre de la tienda no puede exceder 100 caracteres")]
    public string Tienda { get; set; } = string.Empty;

    [Required(ErrorMessage = "El mes es requerido")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12")]
    public int Mes { get; set; }

    [Required(ErrorMessage = "El año es requerido")]
    [Range(2020, 2030, ErrorMessage = "El año debe estar entre 2020 y 2030")]
    public int Año { get; set; }

    [Required(ErrorMessage = "El monto facturado es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El monto facturado debe ser mayor o igual a 0")]
    public decimal MontoFacturado { get; set; }

    [Required(ErrorMessage = "Las unidades vendidas son requeridas")]
    [Range(0, int.MaxValue, ErrorMessage = "Las unidades vendidas deben ser mayor o igual a 0")]
    public int UnidadesVendidas { get; set; }

    public List<string> TopProductos { get; set; } = new();
    public List<string> TopCategorias { get; set; } = new();
}

// DTO para actualizar venta existente (Request)
public class UpdateVentaWooCommerceDto
{
    [Required(ErrorMessage = "El ID es requerido")]
    public int Id { get; set; }

    [Required(ErrorMessage = "La tienda es requerida")]
    [StringLength(100, ErrorMessage = "El nombre de la tienda no puede exceder 100 caracteres")]
    public string Tienda { get; set; } = string.Empty;

    [Required(ErrorMessage = "El mes es requerido")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12")]
    public int Mes { get; set; }

    [Required(ErrorMessage = "El año es requerido")]
    [Range(2020, 2030, ErrorMessage = "El año debe estar entre 2020 y 2030")]
    public int Año { get; set; }

    [Required(ErrorMessage = "El monto facturado es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El monto facturado debe ser mayor o igual a 0")]
    public decimal MontoFacturado { get; set; }

    [Required(ErrorMessage = "Las unidades vendidas son requeridas")]
    [Range(0, int.MaxValue, ErrorMessage = "Las unidades vendidas deben ser mayor o igual a 0")]
    public int UnidadesVendidas { get; set; }

    public List<string> TopProductos { get; set; } = new();
    public List<string> TopCategorias { get; set; } = new();
}

// DTO para consultas con filtros
public class VentaWooCommerceQueryDto
{
    public string? Tienda { get; set; }
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

// DTO para resumen de ventas (como en tu Excel)
public class ResumenVentasDto
{
    public string Tienda { get; set; } = string.Empty;
    public decimal MontoFacturado { get; set; }
    public int UnidadesVendidas { get; set; }
    public List<string> TopProductos { get; set; } = new();
    public List<string> TopCategorias { get; set; } = new();
}

// DTO para totales generales
public class TotalesVentasDto
{
    public decimal TotalFacturado { get; set; }
    public int TotalUnidades { get; set; }
    public List<ResumenVentasDto> VentasPorTienda { get; set; } = new();
    public int Mes { get; set; }
    public int Año { get; set; }
    public string PeriodoCompleto { get; set; } = string.Empty;
}