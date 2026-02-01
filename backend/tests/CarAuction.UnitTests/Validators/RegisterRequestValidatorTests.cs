using CarAuction.Application.DTOs.Auth;
using CarAuction.Application.Validators;

namespace CarAuction.UnitTests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void Should_HaveError_When_EmailIsEmpty()
    {
        // Arrange
        var request = new RegisterRequest { Email = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("El correo electrónico es requerido");
    }

    [Fact]
    public void Should_HaveError_When_EmailIsInvalid()
    {
        // Arrange
        var request = new RegisterRequest { Email = "invalid-email" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("El correo electrónico no es válido");
    }

    [Theory]
    [InlineData("short")]           // Too short (< 8 chars)
    [InlineData("nouppercase1")]    // No uppercase
    [InlineData("NOLOWERCASE1")]    // No lowercase
    [InlineData("NoNumbers")]       // No numbers
    public void Should_HaveError_When_PasswordIsWeak(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Password = password,
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_HaveError_When_PasswordsDoNotMatch()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPass123",
            ConfirmPassword = "DifferentPass123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Las contraseñas no coinciden");
    }

    [Fact]
    public void Should_HaveError_When_FirstNameIsEmpty()
    {
        // Arrange
        var request = new RegisterRequest { FirstName = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("El nombre es requerido");
    }

    [Fact]
    public void Should_HaveError_When_LastNameIsEmpty()
    {
        // Arrange
        var request = new RegisterRequest { LastName = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("El apellido es requerido");
    }

    [Fact]
    public void Should_NotHaveErrors_When_RequestIsValid()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "valid@example.com",
            Password = "ValidPassword123",
            ConfirmPassword = "ValidPassword123",
            FirstName = "Juan",
            LastName = "Pérez",
            PhoneNumber = "+52 555 123 4567"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_When_PhoneNumberIsNull()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "valid@example.com",
            Password = "ValidPassword123",
            ConfirmPassword = "ValidPassword123",
            FirstName = "Juan",
            LastName = "Pérez",
            PhoneNumber = null
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
}
