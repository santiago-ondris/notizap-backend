public interface IEvolucionStockService
{
    List<EvolucionStockPorPuntoDeVentaDto> CalcularEvolucionStock(
        List<CompraDetalleConFechaDto> compras,
        List<VentaDto> ventas,
        string productoBase
    );
}
