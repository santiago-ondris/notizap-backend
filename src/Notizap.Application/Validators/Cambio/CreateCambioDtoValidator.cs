using FluentValidation;

public class CreateCambioDtoValidator : AbstractValidator<CreateCambioDto>
{
    public CreateCambioDtoValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.Pedido)
            .NotEmpty().WithMessage("El número de pedido es obligatorio.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio.");

        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido del cliente es obligatorio.");

        RuleFor(x => x.DNI)
            .NotEmpty().WithMessage("El DNI es obligatorio.")
            .Matches(@"^\d{7,8}$").WithMessage("El DNI debe tener 7 u 8 dígitos numéricos.");

        RuleFor(x => x.Celular)
            .NotEmpty().WithMessage("El número de celular es obligatorio.");

        RuleFor(x => x.ModeloOriginal)
            .NotEmpty().WithMessage("El modelo original es obligatorio.");

        RuleFor(x => x.ModeloCambio)
            .NotEmpty().WithMessage("El modelo de cambio es obligatorio.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo del cambio es obligatorio.");

        RuleFor(x => x.Calle)
            .NotEmpty().WithMessage("La calle de entrega es obligatoria.");

        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("El número de domicilio es obligatorio.");

        RuleFor(x => x.CodigoPostal)
            .NotEmpty().WithMessage("El código postal es obligatorio.");

        RuleFor(x => x.Localidad)
            .NotEmpty().WithMessage("La localidad es obligatoria.");

        RuleFor(x => x.Provincia)
            .NotEmpty().WithMessage("La provincia es obligatoria.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Alto)
            .GreaterThan(0).WithMessage("El alto debe ser mayor a 0.");

        RuleFor(x => x.Ancho)
            .GreaterThan(0).WithMessage("El ancho debe ser mayor a 0.");

        RuleFor(x => x.Largo)
            .GreaterThan(0).WithMessage("El largo debe ser mayor a 0.");

        RuleFor(x => x.Peso)
            .GreaterThan(0).WithMessage("El peso debe ser mayor a 0.");

        RuleFor(x => x.ValorDeclarado)
            .GreaterThanOrEqualTo(0).WithMessage("El valor declarado no puede ser negativo.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("Debe haber al menos un paquete.");

        RuleFor(x => x.NumeroRemito)
            .NotEmpty().WithMessage("El número de remito es obligatorio.");

        RuleFor(x => x.IdOperativa)
            .GreaterThan(0).WithMessage("El ID de operativa es obligatorio.");

        RuleFor(x => x.IdCentroImposicionOrigen)
            .GreaterThan(0).WithMessage("El ID del centro de imposición de origen es obligatorio.");

        RuleFor(x => x.IdCentroImposicionDestino)
            .GreaterThan(0).WithMessage("El ID del centro de imposición destino es obligatorio.");

        RuleFor(x => x.IdFranjaHoraria)
            .InclusiveBetween(1, 3).WithMessage("La franja horaria debe ser 1, 2 o 3.");

        RuleFor(x => x.FechaRetiro)
            .NotEmpty().WithMessage("La fecha de retiro es obligatoria.");

        RuleFor(x => x.Responsable)
            .NotEmpty().WithMessage("Debe asignarse un responsable.");
    }
}
