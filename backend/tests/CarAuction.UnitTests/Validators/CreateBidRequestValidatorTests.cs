using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.Validators;

namespace CarAuction.UnitTests.Validators;

public class CreateBidRequestValidatorTests
{
    private readonly CreateBidRequestValidator _validator;

    public CreateBidRequestValidatorTests()
    {
        _validator = new CreateBidRequestValidator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_HaveError_When_AuctionIdIsInvalid(int auctionId)
    {
        // Arrange
        var request = new CreateBidRequest
        {
            AuctionId = auctionId,
            Amount = 10000
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AuctionId)
            .WithErrorMessage("La subasta es requerida");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Should_HaveError_When_AmountIsInvalid(decimal amount)
    {
        // Arrange
        var request = new CreateBidRequest
        {
            AuctionId = 1,
            Amount = amount
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("El monto de la puja debe ser mayor a 0");
    }

    [Fact]
    public void Should_NotHaveErrors_When_RequestIsValid()
    {
        // Arrange
        var request = new CreateBidRequest
        {
            AuctionId = 1,
            Amount = 15000.50m
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1, 100)]
    [InlineData(999, 999999.99)]
    [InlineData(1, 0.01)]
    public void Should_NotHaveErrors_When_ValidValues(int auctionId, decimal amount)
    {
        // Arrange
        var request = new CreateBidRequest
        {
            AuctionId = auctionId,
            Amount = amount
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
