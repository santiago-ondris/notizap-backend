using FluentValidation;

namespace Notizap.Application.Validators.Gastos
{
    public class CreateGastoDtoValidator : AbstractValidator<CreateGastoDto>
    {
        public CreateGastoDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no debe superar los 100 caracteres.");

            RuleFor(x => x.Descripcion)
                .MaximumLength(500).WithMessage("La descripción no debe superar los 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Descripcion));

            RuleFor(x => x.Categoria)
                .NotEmpty().WithMessage("La categoría es obligatoria.")
                .MaximumLength(50).WithMessage("La categoría no debe superar los 50 caracteres.");

            RuleFor(x => x.Monto)
                .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.")
                .LessThanOrEqualTo(999999.99m).WithMessage("El monto no puede superar $999,999.99");

            RuleFor(x => x.Fecha)
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("La fecha no puede ser futura.")
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-5)).WithMessage("La fecha no puede ser mayor a 5 años atrás.")
                .When(x => x.Fecha.HasValue);

            RuleFor(x => x.Proveedor)
                .MaximumLength(100).WithMessage("El proveedor no debe superar los 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Proveedor));

            RuleFor(x => x.MetodoPago)
                .MaximumLength(50).WithMessage("El método de pago no debe superar los 50 caracteres.")
                .Must(BeValidMetodoPago).WithMessage("Método de pago no válido. Use: Efectivo, Tarjeta de Crédito, Tarjeta de Débito, Transferencia, Cheque, Otros")
                .When(x => !string.IsNullOrEmpty(x.MetodoPago));

            RuleFor(x => x.FrecuenciaRecurrencia)
                .Must(BeValidFrecuencia).WithMessage("Frecuencia no válida. Use: mensual, bimestral, trimestral, semestral, anual")
                .When(x => x.EsRecurrente && !string.IsNullOrEmpty(x.FrecuenciaRecurrencia));

            RuleFor(x => x.FrecuenciaRecurrencia)
                .NotEmpty().WithMessage("Debe especificar la frecuencia para gastos recurrentes.")
                .When(x => x.EsRecurrente);
        }

        private bool BeValidMetodoPago(string? metodoPago)
        {
            if (string.IsNullOrEmpty(metodoPago)) return true;

            var metodosValidos = new[] 
            { 
                "Efectivo", 
                "Tarjeta de Crédito", 
                "Tarjeta de Débito", 
                "Transferencia", 
                "Cheque", 
                "Otros" 
            };

            return metodosValidos.Contains(metodoPago, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeValidFrecuencia(string? frecuencia)
        {
            if (string.IsNullOrEmpty(frecuencia)) return true;

            var frecuenciasValidas = new[] 
            { 
                "mensual", 
                "bimestral", 
                "trimestral", 
                "semestral", 
                "anual" 
            };

            return frecuenciasValidas.Contains(frecuencia, StringComparer.OrdinalIgnoreCase);
        }
    }
}