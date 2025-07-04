public class CreateEnvioDiarioDto
{
    public DateTime Fecha { get; set; }

    public int Oca { get; set; }
    public int Andreani { get; set; }
    public int RetirosSucursal { get; set; }

    public int Roberto { get; set; }
    public int Tino { get; set; }
    public int Caddy { get; set; }

    public int MercadoLibre { get; set; }
}

public class GuardarEnviosLoteDto
{
    public List<CreateEnvioDiarioDto> Envios { get; set; } = new();
}

public class ResultadoLoteDto
{
    public int Exitosos { get; set; }
    public int Fallidos { get; set; }
    public List<string> Errores { get; set; } = new();
    public string Mensaje { get; set; } = string.Empty;
    public bool TodosExitosos => Fallidos == 0;
}
