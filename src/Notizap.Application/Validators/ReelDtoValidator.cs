using FluentValidation;

public class ReelDtoValidator : AbstractValidator<ReelDto>
{
    public ReelDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(100).WithMessage("El título no puede superar los 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(300).WithMessage("La descripción no puede superar los 300 caracteres.");

        RuleFor(x => x.Views)
            .GreaterThanOrEqualTo(0).WithMessage("Las vistas no pueden ser negativas.");

        RuleFor(x => x.Likes)
            .GreaterThanOrEqualTo(0).WithMessage("Los likes no pueden ser negativos.");

        RuleFor(x => x.Comments)
            .GreaterThanOrEqualTo(0).WithMessage("Los comentarios no pueden ser negativos.");

        RuleFor(x => x.ThumbnailUrl)
            .NotEmpty().WithMessage("La URL de la miniatura es obligatoria.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("La URL de la miniatura debe ser válida.");

        RuleFor(x => x.InstagramUrl)
            .NotEmpty().WithMessage("La URL de Instagram es obligatoria.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("La URL de Instagram debe ser válida.");
    }
}
