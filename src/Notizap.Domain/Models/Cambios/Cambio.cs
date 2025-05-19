public class Cambio
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Pedido { get; set; } = default!;
    public string Celular { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string Apellido { get; set; } = default!;
    public string DNI { get; set; } = default!;
    public string ModeloOriginal { get; set; } = default!;
    public string ModeloCambio { get; set; } = default!;
    public string Motivo { get; set; } = default!;
    public bool ParPedido { get; set; }
    public bool LlegoAlDeposito { get; set; }
    public bool YaEnviado { get; set; }
    public decimal? DiferenciaAbonada { get; set; }
    public decimal? DiferenciaAFavor { get; set; }
    public string Envio { get; set; } = default!;
    public bool CambioRegistradoSistema { get; set; }

    // Datos del domicilio de entrega
    public string Calle { get; set; } = default!;
    public string Numero { get; set; } = default!;
    public string? Piso { get; set; }
    public string? Departamento { get; set; }
    public string CodigoPostal { get; set; } = default!;
    public string Localidad { get; set; } = default!;
    public string Provincia { get; set; } = default!;
    public string Email { get; set; } = default!;

    // Datos del paquete
    public decimal Alto { get; set; }
    public decimal Ancho { get; set; }
    public decimal Largo { get; set; }
    public decimal Peso { get; set; }
    public decimal ValorDeclarado { get; set; }
    public int Cantidad { get; set; }

    // Datos de envío
    public string Observaciones { get; set; } = string.Empty;
    public string NumeroRemito { get; set; } = default!;
    public int IdOperativa { get; set; }
    public int IdCentroImposicionOrigen { get; set; }
    public int IdCentroImposicionDestino { get; set; }
    public int IdFranjaHoraria { get; set; }
    public DateTime FechaRetiro { get; set; }
    public string? TrackingNumber { get; set; }
    public string? UrlEtiquetaPdf { get; set; }
    public int? IdOrdenRetiro { get; set; }

    // Metadatos
    public string Responsable { get; set; } = default!;
}
