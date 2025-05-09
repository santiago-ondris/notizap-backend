using FluentValidation;
using System;

public class MercadoLibreManualDtoValidator : AbstractValidator<MercadoLibreManualDto>
{
    public MercadoLibreManualDtoValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2023, DateTime.UtcNow.Year)
            .WithMessage("El año debe estar entre 2023 y el año actual.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("El mes debe estar entre 1 y 12.");

        RuleFor(x => x.UnitsSold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Las unidades vendidas no pueden ser negativas.");

        RuleFor(x => x.Revenue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La ganancia no puede ser negativa.");
    }
}
