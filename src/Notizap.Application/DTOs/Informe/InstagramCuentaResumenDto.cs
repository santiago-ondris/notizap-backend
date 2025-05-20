public class ReelTopResumen
{
    public DateTime FechaPublicacion { get; set; }
    public int Views { get; set; }
    public int Likes { get; set; }
}

public class InstagramCuentaResumenDto
{
    public ReelTopResumen? MasVisto { get; set; }
    public ReelTopResumen? MasLikeado { get; set; }

    public int CantidadReels { get; set; }
    public int CantidadPosteos { get; set; }
    public int CantidadHistorias { get; set; }
    public int NuevosSeguidores { get; set; }
}
