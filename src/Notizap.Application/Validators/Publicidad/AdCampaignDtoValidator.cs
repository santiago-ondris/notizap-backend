using FluentValidation;

public class AdCampaignDtoValidator : AbstractValidator<AdCampaignDto>
{
    public AdCampaignDtoValidator()
    {
        RuleFor(c => c.Nombre)
            .NotEmpty().WithMessage("El nombre de la campaña es obligatorio.");

        RuleFor(c => c.Tipo)
            .NotEmpty().WithMessage("El tipo de campaña es obligatorio.");

        RuleFor(c => c.MontoInvertido)
            .GreaterThan(0).WithMessage("El monto invertido debe ser mayor a 0.");

        RuleFor(c => c.Objetivo)
            .NotEmpty().WithMessage("El objetivo es obligatorio.");

        RuleFor(c => c.Resultados)
            .NotEmpty().WithMessage("Los resultados son obligatorios.");

        RuleFor(c => c.FechaInicio)
            .LessThanOrEqualTo(c => c.FechaFin)
            .WithMessage("La fecha de inicio no puede ser posterior a la de fin.");

        RuleFor(c => c.FechaFin)
            .GreaterThanOrEqualTo(c => c.FechaInicio)
            .WithMessage("La fecha de fin no puede ser anterior a la de inicio.");
    }
}
