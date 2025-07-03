using System.Text.Json;
public static class VentaWooCommerceHelpers
{
    // Manejo de JSON para productos y categorías
    public static string SerializeList(List<string> items)
    {
        if (items == null || !items.Any())
            return "[]";
            
        return JsonSerializer.Serialize(items, new JsonSerializerOptions 
        { 
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public static List<string> DeserializeList(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
            return new List<string>();

        try
        {
            var items = JsonSerializer.Deserialize<List<string>>(json);
            return items ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    // Validaciones de negocio
    public static bool EsPeriodoValido(int mes, int año)
    {
        return mes >= 1 && mes <= 12 && año >= 2020 && año <= 2030;
    }

    public static bool EsFechaFutura(int mes, int año)
    {
        var fechaActual = DateTime.UtcNow;
        var fechaConsulta = new DateTime(año, mes, 1);
        return fechaConsulta > fechaActual;
    }

    // Normalización de datos
    public static string NormalizarTienda(string tienda)
    {
        if (string.IsNullOrWhiteSpace(tienda))
            return string.Empty;

        return tienda.Trim().ToUpperInvariant();
    }

    public static List<string> LimpiarYValidarLista(List<string> items, int maxItems = 10)
    {
        if (items == null)
            return new List<string>();

        return items
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Where(item => item.Length > 0)
            .Take(maxItems)
            .ToList();
    }

    // Cálculos de crecimiento
    public static decimal CalcularCrecimientoPorcentual(decimal valorActual, decimal valorAnterior)
    {
        if (valorAnterior == 0)
            return valorActual > 0 ? 100 : 0;

        return Math.Round(((valorActual - valorAnterior) / valorAnterior) * 100, 2);
    }

    // Formateo de períodos
    public static string FormatearPeriodo(int mes, int año)
    {
        return $"{mes:D2}/{año}";
    }

    public static string FormatearPeriodoCompleto(int mes, int año)
    {
        var nombresMeses = new string[]
        {
            "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
            "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
        };

        if (mes < 1 || mes > 12)
            return $"{mes:D2}/{año}";

        return $"{nombresMeses[mes]} {año}";
    }

    // Validaciones de consultas
    public static VentaWooCommerceQueryDto ValidarYLimpiarQuery(VentaWooCommerceQueryDto query)
    {
        if (query == null)
            return new VentaWooCommerceQueryDto();

        // Normalizar tienda
        if (!string.IsNullOrWhiteSpace(query.Tienda))
            query.Tienda = NormalizarTienda(query.Tienda);

        // Validar paginación
        if (query.PageNumber < 1)
            query.PageNumber = 1;

        if (query.PageSize < 1 || query.PageSize > 100)
            query.PageSize = 10;

        // Validar ordenamiento
        var orderByValidos = new[] { "FechaCreacion", "Mes", "Año", "MontoFacturado", "UnidadesVendidas", "Tienda" };
        if (string.IsNullOrWhiteSpace(query.OrderBy) || !orderByValidos.Contains(query.OrderBy))
            query.OrderBy = "FechaCreacion";

        return query;
    }

    // Conversión de períodos
    public static (int mes, int año) ObtenerPeriodoAnterior(int mes, int año)
    {
        if (mes == 1)
            return (12, año - 1);
        
        return (mes - 1, año);
    }

    public static (int mes, int año) ObtenerPeriodoSiguiente(int mes, int año)
    {
        if (mes == 12)
            return (1, año + 1);
        
        return (mes + 1, año);
    }

    // Generación de resúmenes
    public static TotalesVentasDto GenerarTotales(IEnumerable<VentaWooCommerceDto> ventas, int mes, int año)
    {
        if (ventas == null || !ventas.Any())
        {
            return new TotalesVentasDto
            {
                Mes = mes,
                Año = año,
                PeriodoCompleto = FormatearPeriodo(mes, año)
            };
        }

        var ventasList = ventas.ToList();
        
        return new TotalesVentasDto
        {
            TotalFacturado = ventasList.Sum(v => v.MontoFacturado),
            TotalUnidades = ventasList.Sum(v => v.UnidadesVendidas),
            VentasPorTienda = ventasList.Select(v => new ResumenVentasDto
            {
                Tienda = v.Tienda,
                MontoFacturado = v.MontoFacturado,
                UnidadesVendidas = v.UnidadesVendidas,
                TopProductos = v.TopProductos,
                TopCategorias = v.TopCategorias
            }).ToList(),
            Mes = mes,
            Año = año,
            PeriodoCompleto = FormatearPeriodo(mes, año)
        };
    }

    // Validación de duplicados
    public static bool EsDuplicado(CreateVentaWooCommerceDto nuevo, IEnumerable<VentaWooCommerceDto> existentes)
    {
        if (existentes == null || !existentes.Any())
            return false;

        var tiendaNormalizada = NormalizarTienda(nuevo.Tienda);
        
        return existentes.Any(v => 
            NormalizarTienda(v.Tienda) == tiendaNormalizada && 
            v.Mes == nuevo.Mes && 
            v.Año == nuevo.Año);
    }
}