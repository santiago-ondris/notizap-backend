using FluentValidation;

public class CreateProductAdDtoValidator : AbstractValidator<CreateProductAdDto>
{
    public CreateProductAdDtoValidator()
    {
        RuleFor(x => x.NombreCampania).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2020, DateTime.UtcNow.Year);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Inversion).GreaterThan(0);
    }
}