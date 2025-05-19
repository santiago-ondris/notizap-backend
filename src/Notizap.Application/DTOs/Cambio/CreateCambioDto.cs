public class CreateCambioDto
{
    public DateTime Fecha { get; set; }
    public string Pedido { get; set; } = null!;
    public string Celular { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Apellido { get; set; }
    public string DNI { get; set; } = null!;
    public string ModeloOriginal { get; set; } = null!;
    public string ModeloCambio { get; set; } = null!;
    public string Motivo { get; set; } = null!;
    public bool ParPedido { get; set; }
    public decimal? DiferenciaAbonada { get; set; }
    public decimal? DiferenciaAFavor { get; set; }
    public string Envio { get; set; } = null!;

    public string Calle { get; set; } = null!;
    public string Numero { get; set; } = null!;
    public string? Piso { get; set; }
    public string? Departamento { get; set; }
    public string CodigoPostal { get; set; } = null!;
    public string Localidad { get; set; } = null!;
    public string Provincia { get; set; } = null!;
    public string Email { get; set; } = null!;

    public decimal Alto { get; set; }
    public decimal Ancho { get; set; }
    public decimal Largo { get; set; }
    public decimal Peso { get; set; }
    public decimal ValorDeclarado { get; set; }
    public int Cantidad { get; set; }

    public string? Observaciones { get; set; }
    public string NumeroRemito { get; set; } = null!;
    public int IdOperativa { get; set; }
    public int IdCentroImposicionOrigen { get; set; }
    public int IdCentroImposicionDestino { get; set; }
    public int IdFranjaHoraria { get; set; }
    public DateTime FechaRetiro { get; set; }

    public string? Responsable { get; set; }
}
