using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

namespace Notizap.Services.Analisis
{
    public class EvolucionVentasService : IEvolucionVentasService
    {
        private static readonly Dictionary<string, string> SucursalDic = new()
        {
            { "0001", "General Paz" },
            { "0096", "General Paz" },
            { "0002", "Dean Funes" },
            { "0005", "Dean Funes" },
            { "0095", "Dean Funes" },
            { "0003", "Peatonal" },
            { "0020", "Peatonal" },
            { "0004", "Nueva Cordoba" },
            { "0092", "Nueva Cordoba" },
            { "0006", "E-Commerce" },
            { "0098", "E-Commerce" },
            { "0099", "Casa Central" },
            { "0007", "Casa Central" }
        };

        public EvolucionVentasResponse CalcularEvolucionVentas(IFormFile archivoVentas)
        {
            // 1. Leer ventas del archivo
            var ventas = LeerVentas(archivoVentas);

            // 2. Detectar fechas límites del archivo
            var fechaMin = ventas.Min(v => v.Fecha.Date);
            var fechaMax = ventas.Max(v => v.Fecha.Date);
            var fechas = Enumerable.Range(0, (fechaMax - fechaMin).Days + 1)
                                   .Select(d => fechaMin.AddDays(d).ToString("yyyy-MM-dd"))
                                   .ToList();

            // 3. Productos únicos
            var productosUnicos = ventas.Select(v => v.Producto).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();

            var resultado = new EvolucionVentasResponse
            {
                Fechas = fechas
            };

            foreach (var producto in productosUnicos)
            {
                var productoVentas = ventas.Where(v => v.Producto == producto).ToList();
                var sucursalesUnicas = productoVentas
                    .Select(v => v.Sucursal)
                    .Distinct()
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                var sucursalesDto = new List<EvolucionSucursalDto>();

                // --- Por sucursal ---
                var sucursalesConGlobal = new List<string>(sucursalesUnicas);
                if (!sucursalesConGlobal.Contains("GLOBAL"))
                    sucursalesConGlobal.Add("GLOBAL");

                foreach (var sucursal in sucursalesConGlobal)
                {
                    // Filtrar ventas por sucursal o global
                    List<VentaFlat> ventasSucursal;
                    if (sucursal == "GLOBAL")
                        ventasSucursal = productoVentas;
                    else
                        ventasSucursal = productoVentas.Where(v => v.Sucursal == sucursal).ToList();

                    var serie = SerieAcumuladaPorDia(fechas, ventasSucursal);

                    // Variantes por color en esa sucursal/global
                    var coloresUnicosSucursal = ventasSucursal
                        .Select(v => v.Color)
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .ToList();

                    var variantesSucursal = new List<EvolucionVarianteDto>();
                    if (coloresUnicosSucursal.Count > 1)
                    {
                        foreach (var color in coloresUnicosSucursal)
                        {
                            var ventasColor = ventasSucursal.Where(v => v.Color == color).ToList();
                            var serieColor = SerieAcumuladaPorDia(fechas, ventasColor);
                            variantesSucursal.Add(new EvolucionVarianteDto
                            {
                                Color = color,
                                Serie = serieColor
                            });
                        }
                    }

                    sucursalesDto.Add(new EvolucionSucursalDto
                    {
                        Sucursal = sucursal,
                        Serie = serie,
                        VariantesPorColor = variantesSucursal
                    });
                }

                // --- Serie global (todas las sucursales) ---
                var serieGlobal = SerieAcumuladaPorDia(fechas, productoVentas);
                sucursalesDto.Add(new EvolucionSucursalDto
                {
                    Sucursal = "GLOBAL",
                    Serie = serieGlobal
                });

                var coloresUnicos = productoVentas
                    .Select(v => v.Color)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToList();

                var variantes = new List<EvolucionVarianteDto>();

                if (coloresUnicos.Count > 1)
                {
                    foreach (var color in coloresUnicos)
                    {
                        var ventasColor = productoVentas.Where(v => v.Color == color).ToList();
                        var serieColor = SerieAcumuladaPorDia(fechas, ventasColor);
                        variantes.Add(new EvolucionVarianteDto
                        {
                            Color = color,
                            Serie = serieColor
                        });
                    }
                }

                resultado.Productos.Add(new EvolucionProductoDto
                {
                    Nombre = producto,
                    Sucursales = sucursalesDto,
                    VariantesPorColor = variantes
                });
            }

            return resultado;
        }

        // DTO interno para leer rápido del Excel
        private class VentaFlat
        {
            public DateTime Fecha { get; set; }
            public string Producto { get; set; } = "";
            public string Color { get; set; } = "";    
            public string Sucursal { get; set; } = "";
            public int Cantidad { get; set; }
        }

        private static readonly string[] ProductosExcluidos =
         { 
            "BONIFICACION", "GENERICO", "FABRICACION", "APARADO", "DESCUENTO",
            "STOCK", "VARIOS", "PROMO", "BELT", "CARTERA", "MOCHILA", "TARJETERO",
            "PASHMINA", "CINTO", "BANDOLERA", "BILLETERA", "TOTE", "PARAGUAS", "FICHERO",
            "SEÑA", "MEDIA", "SOQUETE", "CAMPERA", "CUPON", "CARTUCHERA", "PONCHO", "VALIJA",
            "SOBRE", "BOTELLA", "GONDOLA", "ENVIOS", "BAUL", "BOLSO", "GUANTES", "GORRA",
            "LLAVERO", "GORRO", "MALETIN", "CAJA", "RIÑONERA", "PORTA", "CARTU"
        };
        private List<VentaFlat> LeerVentas(IFormFile archivoVentas)
        {
            var ventas = new List<VentaFlat>();

            using (var stream = archivoVentas.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet(1);
                // Buscar header (soporta archivos exportados del ERP)
                var filaHeader = ExcelFinder.EncontrarFilaHeader(ws, new[] { "FECHA", "NRO", "PRODUCTO", "CANT" });
                if (filaHeader == -1)
                    throw new Exception("No se encontró encabezado válido en ventas.");

                var fechaCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "FECHA");
                var nroCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "NRO");
                var productoCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "PRODUCTO");
                var cantidadCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "CANT");

                for (int row = filaHeader + 1; row <= ws.LastRowUsed()!.RowNumber(); row++)
                {
                    var productoFull = ws.Cell(row, productoCol).GetString()?.Trim();
                    if (string.IsNullOrWhiteSpace(productoFull)) continue;

                    if (ProductosExcluidos.Any(excluida =>
                    productoFull.ToUpper().Contains(excluida)))
                    {
                        continue;
                    }

                    var partes = productoFull.Split(" - ");
                    var productoBase = partes[0].Trim();
                    var color = partes.Length >= 2 ? partes[1].Trim() : "";

                    // Fecha
                    var fechaStr = ws.Cell(row, fechaCol).GetString();
                    if (!DateTime.TryParse(fechaStr.Split(' ')[0], out var fecha)) continue;

                    // Sucursal
                    var nro = ws.Cell(row, nroCol).GetString()?.Trim();
                    string sucursal = "";
                    if (!string.IsNullOrEmpty(nro))
                    {
                        var nroSplit = nro.Split("-");
                        if (nroSplit.Length > 1 && SucursalDic.TryGetValue(nroSplit[1], out var nombreSucursal))
                            sucursal = nombreSucursal;
                    }

                    // Cantidad
                    int cantidad = int.TryParse(ws.Cell(row, cantidadCol).GetString(), out var cant) ? cant : 0;

                    ventas.Add(new VentaFlat
                    {
                        Fecha = fecha,
                        Producto = productoBase,
                        Color = color,
                        Sucursal = !string.IsNullOrEmpty(sucursal) ? sucursal : "Sin Sucursal",
                        Cantidad = cantidad
                    });
                }
            }

            return ventas;
        }

        private List<int> SerieAcumuladaPorDia(List<string> fechas, List<VentaFlat> ventas)
        {
            var serie = new List<int>();
            int acumulado = 0;

            var ventasPorFecha = ventas.GroupBy(v => v.Fecha.ToString("yyyy-MM-dd"))
                                       .ToDictionary(g => g.Key, g => g.Sum(v => v.Cantidad));

            foreach (var fecha in fechas)
            {
                if (ventasPorFecha.TryGetValue(fecha, out var cantidadDia))
                    acumulado += cantidadDia;
                serie.Add(acumulado);
            }
            return serie;
        }

        private static string ParsearColor(string productoFull)
        {
            var partes = productoFull.Split(" - ");
            if (partes.Length < 2) return "";
            return partes[1].Trim();
        }

        public EvolucionVentasResumenResponse CalcularEvolucionVentasResumen(IFormFile archivoVentas)
        {
            // 1. Leer ventas del archivo (reutiliza el método existente)
            var ventas = LeerVentas(archivoVentas);

            // 2. Detectar fechas límites del archivo
            var fechaMin = ventas.Min(v => v.Fecha.Date);
            var fechaMax = ventas.Max(v => v.Fecha.Date);
            var fechas = Enumerable.Range(0, (fechaMax - fechaMin).Days + 1)
                                .Select(d => fechaMin.AddDays(d).ToString("yyyy-MM-dd"))
                                .ToList();

            // 3. Agrupar por sucursal y calcular series
            var sucursalesConVentas = ventas
                .Where(v => !string.IsNullOrWhiteSpace(v.Sucursal))
                .GroupBy(v => v.Sucursal)
                .ToList();
            
            var sucursalesDto = new List<EvolucionSucursalResumenDto>();
            
            foreach (var grupo in sucursalesConVentas)
            {
                var serie = SerieAcumuladaPorDia(fechas, grupo.ToList());
                sucursalesDto.Add(new EvolucionSucursalResumenDto
                {
                    Sucursal = grupo.Key,
                    Serie = serie,
                    Color = ObtenerColorSucursal(grupo.Key)
                });
            }
            
            // 4. Agregar serie GLOBAL (suma de todas las sucursales)
            var serieGlobal = SerieAcumuladaPorDia(fechas, ventas);
            sucursalesDto.Add(new EvolucionSucursalResumenDto
            {
                Sucursal = "GLOBAL",
                Serie = serieGlobal,
                Color = "#FF7675"
            });

            return new EvolucionVentasResumenResponse
            {
                Fechas = fechas,
                Sucursales = sucursalesDto
            };
        }

        // Método helper para colores consistentes
        private static string ObtenerColorSucursal(string sucursal)
        {
            return sucursal switch
            {
                "General Paz" => "#E74C3C",      // Rojo intenso
                "Dean Funes" => "#3498DB",       // Azul brillante
                "Peatonal" => "#27AE60",         // Verde puro
                "Nueva Cordoba" => "#F1C40F",    // Amarillo vivo
                "E-Commerce" => "#8E44AD",       // Violeta fuerte
                "Casa Central" => "#FF9800",     // Naranja vibrante
                "Sin Sucursal" => "#1ABC9C",     // Verde agua saturado
                _ => "#34495E"                   // Azul oscuro
            };
        }
    }
}
