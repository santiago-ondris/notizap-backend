using Microsoft.EntityFrameworkCore;

public class ClienteService : IClienteService
{
    private readonly NotizapDbContext _context;

    public ClienteService(NotizapDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ClienteResumenDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Clientes
            .Select(c => new ClienteResumenDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                CantidadCompras = c.CantidadCompras,
                MontoTotalGastado = c.MontoTotalGastado,
                FechaPrimeraCompra = c.FechaPrimeraCompra,
                FechaUltimaCompra = c.FechaUltimaCompra,
                Canales = c.Canales,
                Sucursales = c.Sucursales,
                Observaciones = c.Observaciones,
                Telefono = c.Telefono
            })
            .OrderByDescending(x => x.MontoTotalGastado);

        return await query.ToPagedResultAsync(pageNumber, pageSize);
    }

    // 2. Ficha detallada por cliente
    public async Task<ClienteDetalleDto?> GetByIdAsync(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Compras)
                .ThenInclude(com => com.Detalles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null) return null;

        var topProductos = cliente.Compras
            .SelectMany(c => c.Detalles!)
            .GroupBy(d => d.Producto)
            .Select(g => new TopProductoDto
            {
                Producto = g.Key,
                Cantidad = g.Sum(x => x.Cantidad),
                TotalGastado = g.Sum(x => x.Total)
            })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToList();

        var topCategorias = cliente.Compras
            .SelectMany(c => c.Detalles!)
            .GroupBy(d => d.Categoria)
            .Select(g => new TopCategoriaDto
            {
                Categoria = g.Key,
                Cantidad = g.Sum(x => x.Cantidad),
                TotalGastado = g.Sum(x => x.Total)
            })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToList();

        var topMarcas = cliente.Compras
            .SelectMany(c => c.Detalles!)
            .GroupBy(d => d.Marca)
            .Select(g => new TopMarcaDto
            {
                Marca = g.Key,
                Cantidad = g.Sum(x => x.Cantidad),
                TotalGastado = g.Sum(x => x.Total)
            })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToList();

        var compras = cliente.Compras
            .OrderByDescending(x => x.Fecha)
            .Select(c => new CompraConDetallesDto
            {
                Fecha = c.Fecha,
                Canal = c.Canal,
                Sucursal = c.Sucursal,
                Total = c.Total,
                Detalles = c.Detalles!.Select(d => new DetalleCompraDto
                {
                    Producto = d.Producto,
                    Cantidad = d.Cantidad,
                    Total = d.Total,
                    Marca = d.Marca,
                    Categoria = d.Categoria
                }).ToList()
            }).ToList();

        return new ClienteDetalleDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            CantidadCompras = cliente.CantidadCompras,
            MontoTotalGastado = cliente.MontoTotalGastado,
            FechaPrimeraCompra = cliente.FechaPrimeraCompra,
            FechaUltimaCompra = cliente.FechaUltimaCompra,
            Canales = cliente.Canales,
            Sucursales = cliente.Sucursales,
            Observaciones = cliente.Observaciones,
            Compras = compras,
            TopProductos = topProductos,
            TopCategorias = topCategorias,
            TopMarcas = topMarcas
        };
    }

    public async Task<List<ClienteResumenDto>> GetRankingAsync(
        string ordenarPor = "monto", 
        int top = 10,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? canal = null,
        string? sucursal = null,
        string? marca = null,
        string? categoria = null)
    {
        var query = _context.Clientes
            .Include(c => c.Compras)
            .ThenInclude(c => c.Detalles)
            .AsQueryable();

        // Aplicar los mismos filtros que en FiltrarAsync
        if (!string.IsNullOrWhiteSpace(canal))
        {
            var canalesArray = canal.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(c => c.Trim())
                                .Where(c => !string.IsNullOrEmpty(c))
                                .ToArray();
            
            if (canalesArray.Any())
            {
                query = query.Where(c => canalesArray.Any(canal => c.Canales != null && c.Canales.Contains(canal)));
            }
        }

        if (!string.IsNullOrWhiteSpace(sucursal))
        {
            var sucursalesArray = sucursal.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim())
                                        .Where(s => !string.IsNullOrEmpty(s))
                                        .ToArray();
            
            if (sucursalesArray.Any())
            {
                query = query.Where(c => sucursalesArray.Any(sucursal => c.Sucursales != null && c.Sucursales.Contains(sucursal)));
            }
        }

        if (desde.HasValue)
        {
            desde = DateTime.SpecifyKind(desde.Value, DateTimeKind.Utc);
            query = query.Where(c => c.FechaPrimeraCompra >= desde.Value);
        }

        if (hasta.HasValue)
        {
            hasta = DateTime.SpecifyKind(hasta.Value, DateTimeKind.Utc);
            query = query.Where(c => c.FechaUltimaCompra <= hasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(marca) || !string.IsNullOrWhiteSpace(categoria))
        {
            var marcasArray = string.IsNullOrWhiteSpace(marca) ? Array.Empty<string>() :
                            marca.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(m => m.Trim())
                                .Where(m => !string.IsNullOrEmpty(m))
                                .ToArray();

            var categoriasArray = string.IsNullOrWhiteSpace(categoria) ? Array.Empty<string>() :
                                categoria.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(cat => cat.Trim())
                                        .Where(cat => !string.IsNullOrEmpty(cat))
                                        .ToArray();

            if (marcasArray.Any() || categoriasArray.Any())
            {
                query = query.Where(cliente =>
                    cliente.Compras.Any(compra =>
                        compra.Detalles!.Any(det =>
                            (marcasArray.Length == 0 || marcasArray.Contains(det.Marca)) &&
                            (categoriasArray.Length == 0 || categoriasArray.Contains(det.Categoria))
                        )
                    )
                );
            }
        }

        // Aplicar ordenamiento después de filtrar
        if (ordenarPor == "cantidad")
            query = query.OrderByDescending(c => c.CantidadCompras);
        else
            query = query.OrderByDescending(c => c.MontoTotalGastado);

        // Aplicar el top y mapear a DTO
        return await query.Take(top)
            .Select(c => new ClienteResumenDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                CantidadCompras = c.CantidadCompras,
                MontoTotalGastado = c.MontoTotalGastado,
                FechaPrimeraCompra = c.FechaPrimeraCompra,
                FechaUltimaCompra = c.FechaUltimaCompra,
                Canales = c.Canales,
                Sucursales = c.Sucursales,
                Observaciones = c.Observaciones,
                Telefono = c.Telefono
            })
            .ToListAsync();
    }

    public async Task<List<ClienteResumenDto>> BuscarPorNombreAsync(string filtro)
    {
        filtro = filtro.ToLower();
        return await _context.Clientes
            .Where(c => c.Nombre.ToLower().Contains(filtro))
            .Select(c => new ClienteResumenDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                CantidadCompras = c.CantidadCompras,
                MontoTotalGastado = c.MontoTotalGastado,
                FechaPrimeraCompra = c.FechaPrimeraCompra,
                FechaUltimaCompra = c.FechaUltimaCompra,
                Canales = c.Canales,
                Sucursales = c.Sucursales,
                Observaciones = c.Observaciones,
                Telefono = c.Telefono
            })
            .ToListAsync();
    }

    public async Task<PagedResult<ClienteResumenDto>> FiltrarAsync(
        DateTime? desde, DateTime? hasta, string? canal, string? sucursal, string? marca, string? categoria, int pageNumber = 1, int pageSize = 12)
    {
        var query = _context.Clientes
            .Include(c => c.Compras)
            .ThenInclude(c => c.Detalles)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(canal))
        {
            var canalesArray = canal.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(c => c.Trim())
                                .Where(c => !string.IsNullOrEmpty(c))
                                .ToArray();
            
            if (canalesArray.Any())
            {
                query = query.Where(c => canalesArray.Any(canal => c.Canales != null && c.Canales.Contains(canal)));
            }
        }

        if (!string.IsNullOrWhiteSpace(sucursal))
        {
            var sucursalesArray = sucursal.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim())
                                        .Where(s => !string.IsNullOrEmpty(s))
                                        .ToArray();
            
            if (sucursalesArray.Any())
            {
                query = query.Where(c => sucursalesArray.Any(sucursal => c.Sucursales != null && c.Sucursales.Contains(sucursal)));
            }
        }

        if (desde.HasValue)
        {
            desde = DateTime.SpecifyKind(desde.Value, DateTimeKind.Utc);
            query = query.Where(c => c.FechaPrimeraCompra >= desde.Value);
        }

        if (hasta.HasValue)
        {
            hasta = DateTime.SpecifyKind(hasta.Value, DateTimeKind.Utc);
            query = query.Where(c => c.FechaUltimaCompra <= hasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(marca) || !string.IsNullOrWhiteSpace(categoria))
        {
            var marcasArray = string.IsNullOrWhiteSpace(marca) ? Array.Empty<string>() :
                            marca.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(m => m.Trim())
                                .Where(m => !string.IsNullOrEmpty(m))
                                .ToArray();

            var categoriasArray = string.IsNullOrWhiteSpace(categoria) ? Array.Empty<string>() :
                                categoria.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(cat => cat.Trim())
                                        .Where(cat => !string.IsNullOrEmpty(cat))
                                        .ToArray();

            if (marcasArray.Any() || categoriasArray.Any())
            {
                query = query.Where(cliente =>
                    cliente.Compras.Any(compra =>
                        compra.Detalles!.Any(det =>
                            (marcasArray.Length == 0 || marcasArray.Contains(det.Marca)) &&
                            (categoriasArray.Length == 0 || categoriasArray.Contains(det.Categoria))
                        )
                    )
                );
            }
        }

        var pagedQuery = query.Select(c => new ClienteResumenDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            CantidadCompras = c.CantidadCompras,
            MontoTotalGastado = c.MontoTotalGastado,
            FechaPrimeraCompra = c.FechaPrimeraCompra,
            FechaUltimaCompra = c.FechaUltimaCompra,
            Canales = c.Canales,
            Sucursales = c.Sucursales,
            Observaciones = c.Observaciones,
            Telefono = c.Telefono
        }).OrderByDescending(x => x.MontoTotalGastado);

        return await pagedQuery.ToPagedResultAsync(pageNumber, pageSize);
    }

    public async Task MarcarInactivoAsync(int clienteId)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente != null && (cliente.Observaciones == null || !cliente.Observaciones.Contains("Inactivo")))
        {
            cliente.Observaciones = string.IsNullOrEmpty(cliente.Observaciones)
                ? "Inactivo"
                : cliente.Observaciones + ",Inactivo";
            await _context.SaveChangesAsync();
        }
    }
    public async Task<List<string>> GetCanalesDisponiblesAsync()
    {
        var canales = await _context.Clientes
            .Where(c => !string.IsNullOrEmpty(c.Canales))
            .Select(c => c.Canales)
            .ToListAsync();

        var canalesUnicos = canales
            .SelectMany(canal => canal!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(canal => canal.Trim())
            .Where(canal => !string.IsNullOrEmpty(canal))
            .Distinct()
            .OrderBy(canal => canal)
            .ToList();

        return canalesUnicos;
    }

    public async Task<List<string>> GetSucursalesDisponiblesAsync()
    {
        var sucursales = await _context.Clientes
            .Where(c => !string.IsNullOrEmpty(c.Sucursales))
            .Select(c => c.Sucursales)
            .ToListAsync();

        var sucursalesUnicas = sucursales
            .SelectMany(sucursal => sucursal!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(sucursal => sucursal.Trim())
            .Where(sucursal => !string.IsNullOrEmpty(sucursal))
            .Distinct()
            .OrderBy(sucursal => sucursal)
            .ToList();

        return sucursalesUnicas;
    }

    public async Task<List<string>> GetMarcasDisponiblesAsync()
    {
        var marcas = await _context.Compras
            .SelectMany(c => c.Detalles!)
            .Where(d => !string.IsNullOrEmpty(d.Marca))
            .Select(d => d.Marca)
            .Distinct()
            .OrderBy(marca => marca)
            .ToListAsync();

        return marcas!;
    }

    public async Task<List<string>> GetCategoriasDisponiblesAsync()
    {
        var categorias = await _context.Compras
            .SelectMany(c => c.Detalles!)
            .Where(d => !string.IsNullOrEmpty(d.Categoria))
            .Select(d => d.Categoria)
            .Distinct()
            .OrderBy(categoria => categoria)
            .ToListAsync();

        return categorias!;
    }
    public async Task ActualizarTelefonoAsync(int clienteId, string telefono)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        
        if (cliente == null)
        {
            throw new ArgumentException($"Cliente con ID {clienteId} no encontrado", nameof(clienteId));
        }
        
        cliente.Telefono = telefono;
        await _context.SaveChangesAsync();
    }
}
