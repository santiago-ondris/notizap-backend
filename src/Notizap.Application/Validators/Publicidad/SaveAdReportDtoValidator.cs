using FluentValidation;

public class SaveAdReportDtoValidator : AbstractValidator<SaveAdReportDto>
{
    public SaveAdReportDtoValidator()
    {
        RuleFor(r => r.UnidadNegocio)
            .NotEmpty().WithMessage("La unidad de negocio es obligatoria.")
            .Must(val => val is "Montella" or "Alenka" or "Kids")
            .WithMessage("Unidad de negocio inválida.");

        RuleFor(r => r.Plataforma)
            .NotEmpty().WithMessage("La plataforma es obligatoria.")
            .Must(val => val is "Meta" or "Google")
            .WithMessage("Plataforma inválida.");

        RuleFor(r => r.Year)
            .InclusiveBetween(2020, DateTime.UtcNow.Year + 1)
            .WithMessage("Año inválido.");

        RuleFor(r => r.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Mes inválido.");

        RuleFor(r => r.Campañas)
            .NotNull().WithMessage("Se requiere al menos una campaña.")
            .NotEmpty().WithMessage("Se requiere al menos una campaña.");

        RuleForEach(r => r.Campañas).SetValidator(new AdCampaignDtoValidator());
    }
}
