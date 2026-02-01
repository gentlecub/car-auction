using CarAuction.Application.DTOs.Car;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class CreateCarRequestValidator : AbstractValidator<CreateCarRequest>
{
    public CreateCarRequestValidator()
    {
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("La marca es requerida")
            .MaximumLength(100).WithMessage("La marca no puede exceder 100 caracteres");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("El modelo es requerido")
            .MaximumLength(100).WithMessage("El modelo no puede exceder 100 caracteres");

        RuleFor(x => x.Year)
            .NotEmpty().WithMessage("El año es requerido")
            .InclusiveBetween(1900, DateTime.Now.Year + 1)
            .WithMessage($"El año debe estar entre 1900 y {DateTime.Now.Year + 1}");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo");

        RuleFor(x => x.VIN)
            .Length(17).WithMessage("El VIN debe tener exactamente 17 caracteres")
            .When(x => !string.IsNullOrEmpty(x.VIN));

        RuleFor(x => x.Horsepower)
            .GreaterThan(0).WithMessage("Los caballos de fuerza deben ser mayores a 0")
            .When(x => x.Horsepower.HasValue);
    }
}
