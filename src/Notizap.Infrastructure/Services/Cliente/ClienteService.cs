using ClosedXML.Excel;
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
        string ordenarPor = "montoTotal", 
        int top = 10,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? canal = null,
        string? sucursal = null,
        string? marca = null,
        string? categoria = null,
        bool modoExclusivoCanal = false,     
        bool modoExclusivoSucursal = false,  
        bool modoExclusivoMarca = false,    
        bool modoExclusivoCategoria = false)
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
                if (modoExclusivoCanal)
                {
                    var clientesConCanales = await query
                        .Where(c => c.Canales != null)
                        .Select(c => new { c.Id, c.Canales })
                        .ToListAsync();
                    
                    var idsExclusivos = clientesConCanales
                        .Where(c => 
                        {
                            var canalesCliente = c.Canales!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(canal => canal.Trim())
                                .Where(canal => !string.IsNullOrEmpty(canal))
                                .ToArray();
                            
                            return canalesArray.All(req => canalesCliente.Contains(req)) &&
                                canalesCliente.All(cliente => canalesArray.Contains(cliente));
                        })
                        .Select(c => c.Id)
                        .ToList();
                    
                    query = query.Where(c => idsExclusivos.Contains(c.Id));
                }
                else
                {
                    query = query.Where(c => canalesArray.Any(canal => c.Canales != null && c.Canales.Contains(canal)));
                }
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
                if (modoExclusivoSucursal)
                {
                    var clientesConSucursales = await query
                        .Where(c => c.Sucursales != null)
                        .Select(c => new { c.Id, c.Sucursales })
                        .ToListAsync();
                    
                    var idsExclusivos = clientesConSucursales
                        .Where(c => 
                        {
                            var sucursalesCliente = c.Sucursales!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(sucursal => sucursal.Trim())
                                .Where(sucursal => !string.IsNullOrEmpty(sucursal))
                                .ToArray();
                            
                            return sucursalesArray.All(req => sucursalesCliente.Contains(req)) &&
                                sucursalesCliente.All(cliente => sucursalesArray.Contains(cliente));
                        })
                        .Select(c => c.Id)
                        .ToList();
                    
                    query = query.Where(c => idsExclusivos.Contains(c.Id));
                }
                else
                {
                    query = query.Where(c => sucursalesArray.Any(sucursal => c.Sucursales != null && c.Sucursales.Contains(sucursal)));
                }
            }
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
                if (modoExclusivoMarca || modoExclusivoCategoria)
                {
                    query = query.Where(cliente =>
                        cliente.Compras.Any() && // Tiene compras
                        cliente.Compras.SelectMany(c => c.Detalles!).Any() && // Tiene detalles
                        
                        (marcasArray.Length == 0 || !modoExclusivoMarca || 
                        cliente.Compras.SelectMany(c => c.Detalles!)
                            .Select(d => d.Marca)
                            .Distinct()
                            .All(marcaComprada => marcasArray.Contains(marcaComprada))) &&
                        
                        (categoriasArray.Length == 0 || !modoExclusivoCategoria || 
                        cliente.Compras.SelectMany(c => c.Detalles!)
                            .Select(d => d.Categoria)
                            .Distinct()
                            .All(categoriaComprada => categoriasArray.Contains(categoriaComprada))) &&
                        
                        cliente.Compras.Any(compra =>
                            compra.Detalles!.Any(det =>
                                (marcasArray.Length == 0 || marcasArray.Contains(det.Marca)) &&
                                (categoriasArray.Length == 0 || categoriasArray.Contains(det.Categoria))
                            )
                        )
                    );
                }
                else
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
        }

        var orderedQuery = ExcelFinder.ApplyOrdering(query, ordenarPor, marca, categoria);

        return await orderedQuery.Take(top)
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
        DateTime? desde, DateTime? hasta, string? canal, string? sucursal, string? marca, string? categoria, 
        bool modoExclusivoCanal, bool modoExclusivoSucursal, bool modoExclusivoMarca, bool modoExclusivoCategoria,
        string ordenarPor = "montoTotal",
        int pageNumber = 1, int pageSize = 12)
    {
        var query = _context.Clientes
            .Include(c => c.Compras)
            .ThenInclude(c => c.Detalles)
            .AsQueryable();

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

        if (!string.IsNullOrWhiteSpace(canal))
        {
            var canalesArray = canal.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(c => c.Trim())
                                .Where(c => !string.IsNullOrEmpty(c))
                                .ToArray();
            
            if (canalesArray.Any())
            {
                if (modoExclusivoCanal)
                {
                    var clientesConCanales = await query
                        .Where(c => c.Canales != null)
                        .Select(c => new { c.Id, c.Canales })
                        .ToListAsync();
                    
                    var idsExclusivos = clientesConCanales
                        .Where(c => 
                        {
                            var canalesCliente = c.Canales!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(canal => canal.Trim())
                                .Where(canal => !string.IsNullOrEmpty(canal))
                                .ToArray();
                            
                            return canalesArray.All(req => canalesCliente.Contains(req)) &&
                                canalesCliente.All(cliente => canalesArray.Contains(cliente));
                        })
                        .Select(c => c.Id)
                        .ToList();
                    
                    query = query.Where(c => idsExclusivos.Contains(c.Id));
                }
                else
                {
                    query = query.Where(c => canalesArray.Any(canal => c.Canales != null && c.Canales.Contains(canal)));
                }
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
                if (modoExclusivoSucursal)
                {
                    var clientesConSucursales = await query
                        .Where(c => c.Sucursales != null)
                        .Select(c => new { c.Id, c.Sucursales })
                        .ToListAsync();
                    
                    var idsExclusivos = clientesConSucursales
                        .Where(c => 
                        {
                            var sucursalesCliente = c.Sucursales!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(sucursal => sucursal.Trim())
                                .Where(sucursal => !string.IsNullOrEmpty(sucursal))
                                .ToArray();
                            
                            return sucursalesArray.All(req => sucursalesCliente.Contains(req)) &&
                                sucursalesCliente.All(cliente => sucursalesArray.Contains(cliente));
                        })
                        .Select(c => c.Id)
                        .ToList();
                    
                    query = query.Where(c => idsExclusivos.Contains(c.Id));
                }
                else
                {
                    query = query.Where(c => sucursalesArray.Any(sucursal => c.Sucursales != null && c.Sucursales.Contains(sucursal)));
                }
            }
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
                if (modoExclusivoMarca || modoExclusivoCategoria)
                {
                    query = query.Where(cliente =>
                        cliente.Compras.Any() && // Tiene compras
                        cliente.Compras.SelectMany(c => c.Detalles!).Any() && // Tiene detalles
                        
                        (marcasArray.Length == 0 || !modoExclusivoMarca || 
                        cliente.Compras.SelectMany(c => c.Detalles!)
                            .Select(d => d.Marca)
                            .Distinct()
                            .All(marcaComprada => marcasArray.Contains(marcaComprada))) &&
                        
                        (categoriasArray.Length == 0 || !modoExclusivoCategoria || 
                        cliente.Compras.SelectMany(c => c.Detalles!)
                            .Select(d => d.Categoria)
                            .Distinct()
                            .All(categoriaComprada => categoriasArray.Contains(categoriaComprada))) &&
                        
                        cliente.Compras.Any(compra =>
                            compra.Detalles!.Any(det =>
                                (marcasArray.Length == 0 || marcasArray.Contains(det.Marca)) &&
                                (categoriasArray.Length == 0 || categoriasArray.Contains(det.Categoria))
                            )
                        )
                    );
                }
                else
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
        }

        var orderedQuery = ExcelFinder.ApplyOrdering(query, ordenarPor, marca, categoria);

        var pagedQuery = orderedQuery.Select(c => new ClienteResumenDto
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
        });

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
    public async Task<byte[]> ExportToExcelAsync(
        DateTime? desde, DateTime? hasta, string? canal, string? sucursal, string? marca, string? categoria,
        bool modoExclusivoCanal, bool modoExclusivoSucursal, bool modoExclusivoMarca, bool modoExclusivoCategoria, string ordenarPor = "montoTotal")
    {
        var query = _context.Clientes
            .Include(c => c.Compras)
            .ThenInclude(c => c.Detalles)
            .AsQueryable();

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

        if (!string.IsNullOrWhiteSpace(canal))
        {
            var canalesArray = canal.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(c => c.Trim())
                                .Where(c => !string.IsNullOrEmpty(c))
                                .ToArray();
            
            if (canalesArray.Any())
            {
                if (modoExclusivoCanal)
                {
                    var clientesConCanales = await query
                        .Where(c => c.Canales != null)
                        .Select(c => new { c.Id, c.Canales })
                        .ToListAsync();
                    
                    var idsExclusivos = clientesConCanales
                        .Where(c => 
                        {
                            var canalesCliente = c.Canales!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(canal => canal.Trim())
                                .Where(canal => !string.IsNullOrEmpty(canal))
                                .ToArray();
                            
                            return canalesArray.All(req => canalesCliente.Contains(req)) &&
                                canalesCliente.All(cliente => canalesArray.Contains(cliente));
                        })
                        .Select(c => c.Id)
                        .ToList();
                    
                    query = query.Where(c => idsExclusivos.Contains(c.Id));
                }
                else
                {
                    query = query.Where(c => canalesArray.Any(canal => c.Canales != null && c.Canales.Contains(canal)));
                }
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
                if (modoExclusivoSucursal)
                {
                    var clientesConSucursales = await query
                        .Where(c => c.Sucursales != null)
                        .Select(c => new { c.Id, c.Sucursales })
                        .ToListAsync();
                    
                    var idsExclusivos = clientesConSucursales
                        .Where(c => 
                        {
                            var sucursalesCliente = c.Sucursales!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(sucursal => sucursal.Trim())
                                .Where(sucursal => !string.IsNullOrEmpty(sucursal))
                                .ToArray();
                            
                            return sucursalesArray.All(req => sucursalesCliente.Contains(req)) &&
                                sucursalesCliente.All(cliente => sucursalesArray.Contains(cliente));
                        })
                        .Select(c => c.Id)
                        .ToList();
                    
                    query = query.Where(c => idsExclusivos.Contains(c.Id));
                }
                else
                {
                    query = query.Where(c => sucursalesArray.Any(sucursal => c.Sucursales != null && c.Sucursales.Contains(sucursal)));
                }
            }
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
                if (modoExclusivoMarca || modoExclusivoCategoria)
                {
                    query = query.Where(cliente =>
                        cliente.Compras.Any() && // Tiene compras
                        cliente.Compras.SelectMany(c => c.Detalles!).Any() && // Tiene detalles
                        
                        (marcasArray.Length == 0 || !modoExclusivoMarca || 
                        cliente.Compras.SelectMany(c => c.Detalles!)
                            .Select(d => d.Marca)
                            .Distinct()
                            .All(marcaComprada => marcasArray.Contains(marcaComprada))) &&
                        
                        (categoriasArray.Length == 0 || !modoExclusivoCategoria || 
                        cliente.Compras.SelectMany(c => c.Detalles!)
                            .Select(d => d.Categoria)
                            .Distinct()
                            .All(categoriaComprada => categoriasArray.Contains(categoriaComprada))) &&
                        
                        cliente.Compras.Any(compra =>
                            compra.Detalles!.Any(det =>
                                (marcasArray.Length == 0 || marcasArray.Contains(det.Marca)) &&
                                (categoriasArray.Length == 0 || categoriasArray.Contains(det.Categoria))
                            )
                        )
                    );
                }
                else
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
        }

        var orderedQuery = ExcelFinder.ApplyOrdering(query, ordenarPor, marca, categoria);

        var clientesParaExport = await orderedQuery
            .Take(100)
            .Select(c => new { 
                Nombre = c.Nombre, 
                TelefonoOriginal = c.Telefono 
            })
            .ToListAsync();

        var clientesConTelefonoFormateado = clientesParaExport.Select(c => new {
            Nombre = c.Nombre,
            Telefono = ExcelFinder.FormatearTelefono(c.TelefonoOriginal)
        }).ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Clientes");
        
        worksheet.Cell(1, 1).Value = "Nombre";
        worksheet.Cell(1, 2).Value = "Teléfono";
        
        worksheet.Range(1, 1, 1, 2).Style.Font.Bold = true;
        worksheet.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        for (int i = 0; i < clientesConTelefonoFormateado.Count; i++)
        {
            worksheet.Cell(i + 2, 1).Value = clientesConTelefonoFormateado[i].Nombre;
            worksheet.Cell(i + 2, 2).Value = clientesConTelefonoFormateado[i].Telefono;
            
            if (!string.IsNullOrEmpty(clientesConTelefonoFormateado[i].Telefono))
            {
                worksheet.Cell(i + 2, 2).Style.NumberFormat.Format = "@"; 
            }
        }
        
        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
