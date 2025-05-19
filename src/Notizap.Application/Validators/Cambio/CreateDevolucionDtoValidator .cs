using FluentValidation;

public class CreateDevolucionDtoValidator : AbstractValidator<CreateDevolucionDto>
{
    public CreateDevolucionDtoValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.Pedido)
            .NotEmpty().WithMessage("El número de pedido es obligatorio.");

        RuleFor(x => x.Celular)
            .NotEmpty().WithMessage("El celular es obligatorio.");

        RuleFor(x => x.Modelo)
            .NotEmpty().WithMessage("El modelo es obligatorio.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo de devolución es obligatorio.");

        RuleFor(x => x.Monto)
            .GreaterThanOrEqualTo(0).When(x => x.Monto.HasValue)
            .WithMessage("El monto debe ser un valor positivo.");

        RuleFor(x => x.PagoEnvio)
            .GreaterThanOrEqualTo(0).When(x => x.PagoEnvio.HasValue)
            .WithMessage("El pago de envío debe ser un valor positivo.");

        RuleFor(x => x.Responsable)
            .NotEmpty().WithMessage("Debe asignarse un responsable.");
    }
}
