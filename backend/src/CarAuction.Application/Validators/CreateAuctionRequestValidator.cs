using CarAuction.Application.DTOs.Auction;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class CreateAuctionRequestValidator : AbstractValidator<CreateAuctionRequest>
{
    public CreateAuctionRequestValidator()
    {
        RuleFor(x => x.CarId)
            .GreaterThan(0).WithMessage("El carro es requerido");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0).WithMessage("El precio inicial debe ser mayor a 0");

        RuleFor(x => x.ReservePrice)
            .GreaterThanOrEqualTo(x => x.StartingPrice)
            .WithMessage("El precio de reserva debe ser mayor o igual al precio inicial")
            .When(x => x.ReservePrice.HasValue);

        RuleFor(x => x.MinimumBidIncrement)
            .GreaterThan(0).WithMessage("El incremento mínimo debe ser mayor a 0");

        RuleFor(x => x.StartTime)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("La fecha de inicio no puede ser en el pasado");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio");

        RuleFor(x => x.ExtensionMinutes)
            .InclusiveBetween(1, 30).WithMessage("La extensión debe ser entre 1 y 30 minutos");

        RuleFor(x => x.ExtensionThresholdMinutes)
            .InclusiveBetween(1, 10).WithMessage("El umbral de extensión debe ser entre 1 y 10 minutos");
    }
}
