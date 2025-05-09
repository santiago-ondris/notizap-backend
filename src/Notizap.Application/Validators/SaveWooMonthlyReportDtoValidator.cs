using FluentValidation;

public class SaveWooMonthlyReportDtoValidator : AbstractValidator<SaveWooMonthlyReportDto>
{
    public SaveWooMonthlyReportDtoValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2023, DateTime.UtcNow.Year)
            .WithMessage("El año debe estar entre 2023 y el año actual.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("El mes debe estar entre 1 y 12.");

        RuleFor(x => x.Store)
            .IsInEnum()
            .WithMessage("La tienda seleccionada no es válida.");

        RuleFor(x => x.UnitsSold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Las unidades vendidas no pueden ser negativas.");

        RuleFor(x => x.Revenue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La ganancia no puede ser negativa.");

        RuleFor(x => x.DailySales)
            .NotNull()
            .WithMessage("Debe proporcionar una lista de ventas diarias.")
            .ForEach(child =>
            {
                child.SetValidator(new DailySalesDtoValidator());
            });
    }
}
