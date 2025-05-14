public class EnvioResumenMensualDto
{
    public int TotalOca { get; set; }
    public int TotalAndreani { get; set; }
    public int TotalRetirosSucursal { get; set; }

    public int TotalRoberto { get; set; }
    public int TotalTino { get; set; }
    public int TotalCaddy { get; set; }

    public int TotalMercadoLibre { get; set; }

    public int TotalCordobaCapital => TotalRoberto + TotalTino + TotalCaddy;
    public int TotalGeneral => TotalOca + TotalAndreani + TotalRetirosSucursal + TotalCordobaCapital + TotalMercadoLibre;
}
