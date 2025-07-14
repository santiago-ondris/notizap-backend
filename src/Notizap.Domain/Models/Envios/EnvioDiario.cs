public class EnvioDiario
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }

    public int? Oca { get; set; }
    public int? Andreani { get; set; }
    public int? RetirosSucursal { get; set; }
    public int? Roberto { get; set; }
    public int? Tino { get; set; }
    public int? Caddy { get; set; }
    public int? MercadoLibre { get; set; }

    public int TotalCordobaCapital => (Roberto ?? 0) + (Tino ?? 0) + (Caddy ?? 0);
    public int TotalEnvios => (Oca ?? 0) + (Andreani ?? 0) + (RetirosSucursal ?? 0) + TotalCordobaCapital + (MercadoLibre ?? 0);
}
