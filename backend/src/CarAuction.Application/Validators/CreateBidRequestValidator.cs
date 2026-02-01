using CarAuction.Application.DTOs.Bid;
using FluentValidation;

namespace CarAuction.Application.Validators;

public class CreateBidRequestValidator : AbstractValidator<CreateBidRequest>
{
    public CreateBidRequestValidator()
    {
        RuleFor(x => x.AuctionId)
            .GreaterThan(0).WithMessage("La subasta es requerida");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto de la puja debe ser mayor a 0");
    }
}
