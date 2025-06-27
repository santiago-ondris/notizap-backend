using ClosedXML.Excel;
public class ExcelVentasProcessor
{
    public class ProcessResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<VentaVendedoraCreateDto> Ventas { get; set; } = new();
        public int TotalFilasProcesadas { get; set; }
        public List<string> Errores { get; set; } = new();
        public DateTime? FechaMinima { get; set; }
        public DateTime? FechaMaxima { get; set; }
    }

    private readonly string[] _columnasRequeridas = { "SUCURSAL", "VENDEDOR", "PRODUCTO", "FECHA", "CANTIDAD", "TOTAL" };

    public async Task<ProcessResult> ProcesarArchivoAsync(Stream archivoStream)
    {
        var resultado = new ProcessResult();
        var ventasTemp = new List<VentaVendedoraCreateDto>();
        var errores = new List<string>();

        try
        {
            using var workbook = new XLWorkbook(archivoStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                resultado.Success = false;
                resultado.Message = "El archivo no contiene hojas de trabajo válidas.";
                return resultado;
            }

            // Buscar la fila del header usando ExcelFinder
            var headerRow = ExcelFinder.EncontrarFilaHeader(worksheet, _columnasRequeridas);
            if (headerRow == -1)
            {
                resultado.Success = false;
                resultado.Message = $"No se encontraron las columnas requeridas: {string.Join(", ", _columnasRequeridas)}";
                return resultado;
            }

            // Obtener índices de columnas
            var colSucursal = ExcelFinder.EncontrarColumna(worksheet, headerRow, "SUCURSAL");
            var colVendedor = ExcelFinder.EncontrarColumna(worksheet, headerRow, "VENDEDOR");
            var colProducto = ExcelFinder.EncontrarColumna(worksheet, headerRow, "PRODUCTO");
            var colFecha = ExcelFinder.EncontrarColumna(worksheet, headerRow, "FECHA");
            var colCantidad = ExcelFinder.EncontrarColumna(worksheet, headerRow, "CANTIDAD");
            var colTotal = ExcelFinder.EncontrarColumna(worksheet, headerRow, "TOTAL");

            // Procesar filas de datos
            var filaActual = headerRow + 1;
            var totalFilas = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            Console.WriteLine($"🔄 [PROCESADOR] Procesando filas {filaActual} a {totalFilas}...");
            var filasVacias = 0;
            const int maxFilasVacias = 10;

            // Variables para mantener la sucursal y vendedor actuales
            string sucursalActual = "";
            string vendedorActual = "";

            while (filasVacias < maxFilasVacias && filaActual <= totalFilas)
            {
                var fila = worksheet.Row(filaActual);
                
                // Obtener valores de sucursal y vendedor de la fila actual
                var sucursalFila = fila.Cell(colSucursal).GetString().Trim();
                var vendedorFila = fila.Cell(colVendedor).GetString().Trim();
                var producto = fila.Cell(colProducto).GetString().Trim();
                
                // Si hay nueva sucursal, actualizarla
                if (!string.IsNullOrWhiteSpace(sucursalFila))
                {
                    sucursalActual = sucursalFila;
                    Console.WriteLine($"🏢 [PROCESADOR] Nueva sucursal: {sucursalActual}");
                }
                
                // Si hay nuevo vendedor, actualizarlo
                if (!string.IsNullOrWhiteSpace(vendedorFila))
                {
                    vendedorActual = vendedorFila;
                    Console.WriteLine($"👤 [PROCESADOR] Nuevo vendedor: {vendedorActual}");
                }
                
                // Log de las primeras 3 filas para debug
                if (filaActual <= headerRow + 3)
                {
                    Console.WriteLine($"📝 [PROCESADOR] Fila {filaActual}: SUC=[{sucursalFila}→{sucursalActual}] VEND=[{vendedorFila}→{vendedorActual}] PROD=[{producto}]");
                }
                
                // Verificar si la fila está vacía (usando los valores actuales)
                if (EsFilaVaciaConValoresActuales(fila, sucursalActual, vendedorActual, producto))
                {
                    Console.WriteLine($"⏭️ [PROCESADOR] Fila {filaActual} vacía, saltando...");
                    filasVacias++;
                    filaActual++;
                    continue;
                }

                filasVacias = 0; // Reset contador de filas vacías
                Console.WriteLine($"✅ [PROCESADOR] Procesando fila {filaActual} con SUC={sucursalActual}, VEND={vendedorActual}...");

                try
                {
                    var venta = ProcesarFilaConValoresActuales(fila, sucursalActual, vendedorActual, colProducto, colFecha, colCantidad, colTotal);
                    if (venta != null)
                    {
                        ventasTemp.Add(venta);
                        Console.WriteLine($"✅ [PROCESADOR] Venta agregada: {venta.VendedorNombre} - {venta.SucursalNombre}");
                        
                        // Actualizar rangos de fecha
                        if (resultado.FechaMinima == null || venta.Fecha < resultado.FechaMinima)
                            resultado.FechaMinima = venta.Fecha;
                        if (resultado.FechaMaxima == null || venta.Fecha > resultado.FechaMaxima)
                            resultado.FechaMaxima = venta.Fecha;
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ [PROCESADOR] Fila {filaActual} retornó null");
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Error en fila {filaActual}: {ex.Message}";
                    errores.Add(error);
                    Console.WriteLine($"❌ [PROCESADOR] {error}");
                }

                filaActual++;
            }

            resultado.TotalFilasProcesadas = filaActual - headerRow - 1;
            resultado.Ventas = ventasTemp;
            resultado.Errores = errores;
            resultado.Success = ventasTemp.Count > 0;
            resultado.Message = resultado.Success 
                ? $"Se procesaron {ventasTemp.Count} ventas correctamente."
                : "No se pudieron procesar ventas del archivo.";

            if (errores.Count > 0)
            {
                resultado.Message += $" Se encontraron {errores.Count} errores.";
            }
        }
        catch (Exception ex)
        {
            resultado.Success = false;
            resultado.Message = $"Error al procesar el archivo: {ex.Message}";
            resultado.Errores.Add(ex.Message);
        }

        return resultado;
    }

    private bool EsFilaVaciaConValoresActuales(IXLRow fila, string sucursalActual, string vendedorActual, string producto)
    {
        // Verificar si es una fila de subtotal
        var esSubTotal = producto.ToUpper().Contains("SUB TOTAL") || 
                        sucursalActual.ToUpper().Contains("SUB TOTAL") ||
                        vendedorActual.ToUpper().Contains("SUB TOTAL");

        // La fila está vacía si no tiene sucursal actual, vendedor actual o producto
        var estaVacia = string.IsNullOrWhiteSpace(sucursalActual) || 
                       string.IsNullOrWhiteSpace(vendedorActual) || 
                       string.IsNullOrWhiteSpace(producto);

        return estaVacia || esSubTotal;
    }

    private bool EsFilaVacia(IXLRow fila, int colSucursal, int colVendedor, int colProducto)
    {
        var sucursal = fila.Cell(colSucursal).GetString().Trim();
        var vendedor = fila.Cell(colVendedor).GetString().Trim();
        var producto = fila.Cell(colProducto).GetString().Trim();

        // Verificar si es una fila de subtotal o vacía
        var esSubTotal = producto.ToUpper().Contains("SUB TOTAL") || 
                        sucursal.ToUpper().Contains("SUB TOTAL") ||
                        vendedor.ToUpper().Contains("SUB TOTAL");

        return string.IsNullOrWhiteSpace(sucursal) && 
                string.IsNullOrWhiteSpace(vendedor) && 
                string.IsNullOrWhiteSpace(producto) ||
                esSubTotal;
    }

    private VentaVendedoraCreateDto? ProcesarFilaConValoresActuales(
        IXLRow fila, string sucursalActual, string vendedorActual, 
        int colProducto, int colFecha, int colCantidad, int colTotal)
    {
        var filaNum = fila.RowNumber();
        var producto = fila.Cell(colProducto).GetString().Trim();
        if (string.IsNullOrWhiteSpace(sucursalActual) ||
            string.IsNullOrWhiteSpace(vendedorActual) ||
            string.IsNullOrWhiteSpace(producto))
            return null;

        var celdaFecha = fila.Cell(colFecha);
        DateTime rawFecha;
        if (celdaFecha.DataType == XLDataType.DateTime)
        {
            rawFecha = celdaFecha.GetDateTime(); // p.ej. 2025-06-05 14:52:20
        }
        else
        {
            var txt = celdaFecha.GetString().Trim();
            if (!DateTime.TryParse(txt, out rawFecha))
                throw new Exception($"Fecha inválida en fila {filaNum}: «{txt}»");
        }

        var offsetArg = TimeSpan.FromHours(-3);
        var fechaOffset = new DateTimeOffset(rawFecha, offsetArg);

        var fechaUtc = fechaOffset.UtcDateTime;

        var cantidadTxt = fila.Cell(colCantidad).GetString().Trim();
        if (!int.TryParse(cantidadTxt, out var cantidad))
            throw new Exception($"Cantidad inválida en fila {filaNum}: «{cantidadTxt}»");

        var totalTxt = fila.Cell(colTotal).GetString().Trim();
        var total = ExcelFinder.ParsearMoneda(totalTxt);
        if (cantidad < 0) total = -total; // descuenta correctamente promociones

        return new VentaVendedoraCreateDto
        {
            SucursalId     = 0,  // se asigna después
            VendedorId     = 0,
            SucursalNombre = sucursalActual,
            VendedorNombre = vendedorActual,
            Producto       = producto,
            Fecha          = fechaUtc,    // <<— UTC corregido
            Cantidad       = cantidad,
            Total          = total
        };
    }

    private VentaVendedoraCreateDto? ProcesarFila(IXLRow fila, int colSucursal, int colVendedor, 
        int colProducto, int colFecha, int colCantidad, int colTotal)
    {
        // Extraer datos básicos
        var sucursalNombre = fila.Cell(colSucursal).GetString().Trim();
        var vendedorNombre = fila.Cell(colVendedor).GetString().Trim();
        var producto = fila.Cell(colProducto).GetString().Trim();

        if (string.IsNullOrWhiteSpace(sucursalNombre) || 
            string.IsNullOrWhiteSpace(vendedorNombre) || 
            string.IsNullOrWhiteSpace(producto))
        {
            return null; // Saltar filas con datos incompletos
        }

        // Procesar fecha
        var celdaFecha = fila.Cell(colFecha);
        DateTime fecha;
        
        if (celdaFecha.DataType == XLDataType.DateTime)
        {
            fecha = celdaFecha.GetDateTime();
        }
        else
        {
            var fechaTexto = celdaFecha.GetString().Trim();
            if (!DateTime.TryParse(fechaTexto, out fecha))
            {
                throw new Exception($"Fecha inválida: {fechaTexto}");
            }
        }

        // Procesar cantidad
        var cantidadTexto = fila.Cell(colCantidad).GetString().Trim();
        if (!int.TryParse(cantidadTexto, out var cantidad))
        {
            throw new Exception($"Cantidad inválida: {cantidadTexto}");
        }

        // Procesar total usando el helper de ExcelFinder
        var totalTexto = fila.Cell(colTotal).GetString().Trim();
        var total = ExcelFinder.ParsearMoneda(totalTexto);

        return new VentaVendedoraCreateDto
        {
            SucursalId = 0, // Se asignará en el servicio principal
            VendedorId = 0, // Se asignará en el servicio principal
            SucursalNombre = sucursalNombre,
            VendedorNombre = vendedorNombre,
            Producto = producto,
            Fecha = fecha,
            Cantidad = cantidad,
            Total = total
        };
    }

    public static bool TieneFechasDuplicadas(List<VentaVendedoraCreateDto> ventasNuevas, 
        DateTime fechaMinExistente, DateTime fechaMaxExistente)
    {
        if (!ventasNuevas.Any()) return false;

        var fechaMinNueva = ventasNuevas.Min(v => v.Fecha.Date);
        var fechaMaxNueva = ventasNuevas.Max(v => v.Fecha.Date);

        // Verificar si hay solapamiento de fechas
        return fechaMinNueva <= fechaMaxExistente.Date && fechaMaxNueva >= fechaMinExistente.Date;
    }

    public static List<string> GetSucursalesDelArchivo(List<VentaVendedoraCreateDto> ventas)
    {
        return ventas.Select(v => v.SucursalNombre)
                    .Distinct()
                    .ToList();
    }

    public static List<string> GetVendedoresDelArchivo(List<VentaVendedoraCreateDto> ventas)
    {
        return ventas.Select(v => v.VendedorNombre)
                    .Distinct()
                    .ToList();
    }
}