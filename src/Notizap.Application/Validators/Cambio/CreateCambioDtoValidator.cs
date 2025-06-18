using FluentValidation;

public class CreateCambioDtoValidator : AbstractValidator<CambioSimpleDto>
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

        RuleFor(x => x.Celular)
            .NotEmpty().WithMessage("El número de celular es obligatorio.");

        RuleFor(x => x.ModeloOriginal)
            .NotEmpty().WithMessage("El modelo original es obligatorio.");

        RuleFor(x => x.ModeloCambio)
            .NotEmpty().WithMessage("El modelo de cambio es obligatorio.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo del cambio es obligatorio.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Responsable)
            .NotEmpty().WithMessage("Debe asignarse un responsable.");
    }
}
