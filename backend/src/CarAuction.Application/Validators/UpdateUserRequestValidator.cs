using CarAuction.Application.DTOs.User;
using FluentValidation;

namespace CarAuction.Application.Validators;

/// <summary>
/// Validator for user profile update requests
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$")
                .WithMessage("El nombre solo puede contener letras")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$")
                .WithMessage("El apellido solo puede contener letras")
                .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("El número de teléfono no puede exceder 20 caracteres")
            .Matches(@"^[\+]?[(]?[0-9]{1,4}[)]?[-\s\./0-9]*$")
                .WithMessage("El formato del número de teléfono es inválido")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
