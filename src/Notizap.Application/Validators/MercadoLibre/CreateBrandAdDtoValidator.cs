using FluentValidation;

public class CreateBrandAdDtoValidator : AbstractValidator<CreateProductAdDto>
{
    public CreateBrandAdDtoValidator()
    {
        RuleFor(x => x.NombreCampania).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2020, DateTime.UtcNow.Year);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Inversion).GreaterThan(0);
    }
}