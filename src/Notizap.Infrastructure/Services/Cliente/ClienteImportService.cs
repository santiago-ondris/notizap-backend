using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ClienteImportService : IClienteImportService
{
    private readonly NotizapDbContext _context;
    private readonly ILogger<ClienteImportService> _logger;

    public ClienteImportService(NotizapDbContext context, ILogger<ClienteImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ImportacionClientesDto> ImportarDesdeExcelAsync(Stream excelFileStream, string nombreArchivo)
    {
        var clientesNuevos = 0;
        var comprasNuevas = 0;
        using var workbook = new XLWorkbook(excelFileStream);
        var ws = workbook.Worksheet(1);

        var headerKeys = new[] { "FECHA", "NRO", "CANAL", "CLIENTE", "PRODUCTO", "MARCA", "CATEGORIA", "CANT", "TOTAL" };
        var filaHeader = ExcelFinder.EncontrarFilaHeader(ws, headerKeys);
        if (filaHeader == -1)
            throw new Exception("No se encontró encabezado válido en el archivo.");

        var colFecha = ExcelFinder.EncontrarColumna(ws, filaHeader, "FECHA");
        var colNro = ExcelFinder.EncontrarColumna(ws, filaHeader, "NRO");
        var colCanal = ExcelFinder.EncontrarColumna(ws, filaHeader, "CANAL");
        var colCliente = ExcelFinder.EncontrarColumna(ws, filaHeader, "CLIENTE");
        var colProducto = ExcelFinder.EncontrarColumna(ws, filaHeader, "PRODUCTO");
        var colMarca = ExcelFinder.EncontrarColumna(ws, filaHeader, "MARCA");
        var colCategoria = ExcelFinder.EncontrarColumna(ws, filaHeader, "CATEGORIA");
        var colCant = ExcelFinder.EncontrarColumna(ws, filaHeader, "CANT");
        var colTotal = ExcelFinder.EncontrarColumna(ws, filaHeader, "TOTAL");

        // Cargar TODOS los clientes de la BD con tracking para poder modificarlos
        var clientesDb = await _context.Clientes
            .ToListAsync(); // Cambié a async y con tracking
        
        // Diccionario para búsqueda rápida por nombre
        var clientesPorNombre = clientesDb
            .GroupBy(c => c.Nombre.Trim().ToLower())
            .ToDictionary(g => g.Key, g => g.First());

        // Diccionario para tracking de clientes procesados en esta importación
        var clientesProcesados = new Dictionary<string, Cliente>();

        for (int row = filaHeader + 1; row <= ws.LastRowUsed()!.RowNumber(); row++)
        {
            var nombreCliente = ws.Cell(row, colCliente).GetString().Trim();
            if (string.IsNullOrWhiteSpace(nombreCliente))
                continue;

            var nombreKey = nombreCliente.Trim().ToLower();

            var canal = ws.Cell(row, colCanal).GetString().Trim();
            var nro = ws.Cell(row, colNro).GetString().Trim();
            var sucursal = MapearSucursal(nro, canal);

            var fechaStr = ws.Cell(row, colFecha).GetString();
            if (!DateTime.TryParse(fechaStr.Split(' ')[0], out var fechaCompra))
                continue;

            fechaCompra = DateTime.SpecifyKind(fechaCompra, DateTimeKind.Utc);

            var producto = ws.Cell(row, colProducto).GetString().Trim();
            var marca = ws.Cell(row, colMarca).GetString().Trim();
            var categoria = ws.Cell(row, colCategoria).GetString().Trim();
            var cantidad = ws.Cell(row, colCant).GetValue<int>();
            var totalStr = ws.Cell(row, colTotal).GetString();
            var total = ExcelFinder.ParsearMoneda(totalStr);
            if (total <= 0)
                continue;

            // Buscar o crear cliente
            Cliente? cliente;
            bool esClienteNuevo = false;
            
            // Primero buscar en los ya procesados
            if (!clientesProcesados.TryGetValue(nombreKey, out cliente))
            {
                // Si no está procesado, buscar en la BD
                if (clientesPorNombre.TryGetValue(nombreKey, out cliente))
                {
                    // Cliente existente en BD
                    _logger.LogInformation("Cliente existente encontrado: {Cliente}", nombreCliente);
                }
                else
                {
                    // Cliente nuevo
                    cliente = new Cliente
                    {
                        Nombre = nombreCliente,
                        CantidadCompras = 0,
                        MontoTotalGastado = 0,
                        FechaPrimeraCompra = fechaCompra,
                        FechaUltimaCompra = fechaCompra,
                        Canales = canal,
                        Sucursales = sucursal,
                        Observaciones = ""
                    };
                    _context.Clientes.Add(cliente);
                    clientesNuevos++;
                    esClienteNuevo = true;
                    _logger.LogInformation("Nuevo cliente creado: {Cliente}", nombreCliente);
                }
                
                // Agregar a procesados
                clientesProcesados[nombreKey] = cliente;
            }

            // Actualizar información del cliente
            if (cliente.FechaPrimeraCompra > fechaCompra) 
                cliente.FechaPrimeraCompra = fechaCompra;
            if (cliente.FechaUltimaCompra < fechaCompra) 
                cliente.FechaUltimaCompra = fechaCompra;
            
            // Actualizar canales únicos
            if (!string.IsNullOrEmpty(cliente.Canales))
            {
                var canalesExistentes = cliente.Canales.Split(',').Select(c => c.Trim()).ToHashSet();
                if (!canalesExistentes.Contains(canal))
                    cliente.Canales += $",{canal}";
            }
            else
            {
                cliente.Canales = canal;
            }
            
            // Actualizar sucursales únicas
            if (!string.IsNullOrEmpty(cliente.Sucursales))
            {
                var sucursalesExistentes = cliente.Sucursales.Split(',').Select(s => s.Trim()).ToHashSet();
                if (!sucursalesExistentes.Contains(sucursal))
                    cliente.Sucursales += $",{sucursal}";
            }
            else
            {
                cliente.Sucursales = sucursal;
            }

            // Crear compra
            var compra = new Compra
            {
                Cliente = cliente,
                Fecha = fechaCompra,
                Canal = canal,
                Sucursal = sucursal,
                Total = total,
                Detalles = new List<CompraDetalle>
                {
                    new CompraDetalle
                    {
                        Producto = producto,
                        Marca = marca,
                        Categoria = categoria,
                        Cantidad = cantidad,
                        Total = total
                    }
                }
            };
            _context.Compras.Add(compra);

            // Actualizar estadísticas del cliente
            cliente.CantidadCompras += 1;
            cliente.MontoTotalGastado += total;
            comprasNuevas++;
        }

        // Guardar cambios
        await _context.SaveChangesAsync();

        // Registrar historial
        _context.HistorialImportacionClientes.Add(new HistorialImportacionClientes
        {
            NombreArchivo = nombreArchivo,
            FechaImportacion = DateTime.UtcNow,
            CantidadClientesNuevos = clientesNuevos,
            CantidadComprasNuevas = comprasNuevas
        });
        await _context.SaveChangesAsync();

        return new ImportacionClientesDto
        {
            NombreArchivo = nombreArchivo,
            FechaImportacion = DateTime.UtcNow,
            CantidadClientesNuevos = clientesNuevos,
            CantidadComprasNuevas = comprasNuevas
        };
    }

    public List<string> ValidarArchivo(Stream excelFileStream)
    {
        var errores = new List<string>();
        using var workbook = new XLWorkbook(excelFileStream);
        var ws = workbook.Worksheet(1);

        var headerKeys = new[] { "FECHA", "NRO", "CANAL", "CLIENTE", "PRODUCTO", "MARCA", "CATEGORIA", "CANT", "TOTAL" };
        var filaHeader = ExcelFinder.EncontrarFilaHeader(ws, headerKeys);
        if (filaHeader == -1)
        {
            errores.Add("No se encontró encabezado válido en el archivo.");
            return errores;
        }

        var colCliente = ExcelFinder.EncontrarColumna(ws, filaHeader, "CLIENTE");
        var colTotal = ExcelFinder.EncontrarColumna(ws, filaHeader, "TOTAL");

        for (int row = filaHeader + 1; row <= ws.LastRowUsed()!.RowNumber(); row++)
        {
            try
            {
                var nombre = ws.Cell(row, colCliente).GetString();
                var totalStr = ws.Cell(row, colTotal).GetString();
                var total = ExcelFinder.ParsearMoneda(totalStr);

                if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(totalStr))
                    continue; 

                if (total <= 0)
                    continue;

                if (string.IsNullOrWhiteSpace(nombre))
                    errores.Add($"Fila {row}: CLIENTE vacío.");
            }
            catch (Exception ex)
            {
                errores.Add($"Fila {row}: Error al leer datos ({ex.Message})");
            }
        }
        return errores;
    }

    // Helper para mapear sucursal usando el diccionario
    private string MapearSucursal(string nro, string canal)
    {
        if (canal != "KIBOO") return "E-Commerce";
        var mapa = new Dictionary<string, string>
        {
            { "0001", "General Paz" }, { "0016", "General Paz" }, { "0096", "General Paz" },
            { "0002", "Dean Funes" }, { "0005", "Dean Funes" }, { "0017", "Dean Funes" }, { "0014", "Dean Funes" }, { "0095", "Dean Funes" },
            { "0003", "Peatonal" }, { "0015", "Peatonal" }, { "0020", "Peatonal" },
            { "0004", "Nueva Cordoba" }, { "0018", "Nueva Cordoba" }, { "0092", "Nueva Cordoba" },
            { "0006", "E-Commerce" }, { "0008", "E-Commerce" }, { "0098", "E-Commerce" },
            { "0099", "Casa Central" }, { "0007", "Casa Central" }
        };
        var partes = nro.Split('-');
        if (partes.Length < 2) 
        {
            _logger.LogWarning("Sucursal desconocida por formato NRO: '{Nro}' Canal: '{Canal}'", nro, canal);
            return "Desconocida";
        }
        if (!mapa.ContainsKey(partes[1]))
        {
            _logger.LogWarning("Sucursal desconocida, código no mapeado: '{CodigoSucursal}' NRO: '{Nro}' Canal: '{Canal}'", partes[1], nro, canal);
            return "Desconocida";
        }
        return mapa[partes[1]];
        }
}