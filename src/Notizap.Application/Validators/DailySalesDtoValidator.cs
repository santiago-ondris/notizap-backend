using FluentValidation;

public class DailySalesDtoValidator : AbstractValidator<DailySalesDto>
{
    public DailySalesDtoValidator()
    {
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.UnitsSold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Las unidades vendidas no pueden ser negativas.");

        RuleFor(x => x.Revenue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La ganancia no puede ser negativa.");
    }
}
