using FluentValidation;

namespace Notizap.Application.Validators.Gastos
{
    public class UpdateGastoDtoValidator : AbstractValidator<UpdateGastoDto>
    {
        public UpdateGastoDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no debe superar los 100 caracteres.");

            RuleFor(x => x.Categoria)
                .NotEmpty().WithMessage("La categoría es obligatoria.")
                .MaximumLength(50).WithMessage("La categoría no debe superar los 50 caracteres.");

            RuleFor(x => x.Monto)
                .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");
        }
    }
}
