using FluentValidation;

public class CreateEnvioDiarioDtoValidator : AbstractValidator<CreateEnvioDiarioDto>
{
    public CreateEnvioDiarioDtoValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.Oca)
            .GreaterThanOrEqualTo(0).WithMessage("OCA no puede ser negativo.");

        RuleFor(x => x.Andreani)
            .GreaterThanOrEqualTo(0).WithMessage("Andreani no puede ser negativo.");

        RuleFor(x => x.RetirosSucursal)
            .GreaterThanOrEqualTo(0).WithMessage("Retiros en sucursal no puede ser negativo.");

        RuleFor(x => x.Roberto)
            .GreaterThanOrEqualTo(0).WithMessage("Roberto no puede ser negativo.");

        RuleFor(x => x.Tino)
            .GreaterThanOrEqualTo(0).WithMessage("Tino no puede ser negativo.");

        RuleFor(x => x.Caddy)
            .GreaterThanOrEqualTo(0).WithMessage("Caddy no puede ser negativo.");

        RuleFor(x => x.MercadoLibre)
            .GreaterThanOrEqualTo(0).WithMessage("Mercado Libre no puede ser negativo.");
    }
}
