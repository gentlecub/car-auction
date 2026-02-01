# Testing

Guía completa de estrategia de testing para el backend CarAuction.

---

## Estado Actual

| Componente | Estado |
|------------|--------|
| Directorio Unit Tests | Creado (vacío) |
| Directorio Integration Tests | Creado (vacío) |
| Tests implementados | 0 |
| Cobertura | 0% |

**Ubicación de directorios:**
```
/backend/tests/
├── CarAuction.UnitTests/
└── CarAuction.IntegrationTests/
```

---

## Estrategia de Testing

### Pirámide de Tests

```
                    ┌───────────┐
                    │   E2E     │  ← Pocos, lentos, costosos
                    │  Tests    │
                   ─┴───────────┴─
                  ┌───────────────┐
                  │  Integration  │  ← Moderados
                  │    Tests      │
                 ─┴───────────────┴─
                ┌───────────────────┐
                │    Unit Tests     │  ← Muchos, rápidos, baratos
                └───────────────────┘
```

### Distribución Recomendada

| Tipo | Proporción | Enfoque |
|------|------------|---------|
| Unit Tests | 70% | Services, Validators, Entities |
| Integration Tests | 25% | Controllers, Database, SignalR |
| E2E Tests | 5% | Flujos críticos completos |

---

## Configuración de Proyectos

### 1. Crear Proyecto Unit Tests

```bash
cd /backend/tests/CarAuction.UnitTests

dotnet new xunit
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package AutoMapper
dotnet add package Microsoft.EntityFrameworkCore.InMemory

# Agregar referencia a proyectos
dotnet add reference ../../src/CarAuction.Domain/CarAuction.Domain.csproj
dotnet add reference ../../src/CarAuction.Application/CarAuction.Application.csproj
dotnet add reference ../../src/CarAuction.Infrastructure/CarAuction.Infrastructure.csproj
```

### 2. Crear Proyecto Integration Tests

```bash
cd /backend/tests/CarAuction.IntegrationTests

dotnet new xunit
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers.MySql
dotnet add package FluentAssertions
dotnet add package Respawn

# Agregar referencia al proyecto API
dotnet add reference ../../src/CarAuction.API/CarAuction.API.csproj
```

### 3. Agregar Proyectos a la Solución

```bash
cd /backend
dotnet sln add tests/CarAuction.UnitTests/CarAuction.UnitTests.csproj
dotnet sln add tests/CarAuction.IntegrationTests/CarAuction.IntegrationTests.csproj
```

---

## Estructura de Proyectos

### Unit Tests

```
/tests/CarAuction.UnitTests/
├── CarAuction.UnitTests.csproj
├── GlobalUsings.cs
├── /Services
│   ├── AuthServiceTests.cs
│   ├── AuctionServiceTests.cs
│   ├── BidServiceTests.cs
│   ├── CarServiceTests.cs
│   └── UserServiceTests.cs
├── /Validators
│   ├── RegisterRequestValidatorTests.cs
│   ├── CreateBidRequestValidatorTests.cs
│   ├── CreateCarRequestValidatorTests.cs
│   └── CreateAuctionRequestValidatorTests.cs
├── /Entities
│   ├── UserTests.cs
│   ├── AuctionTests.cs
│   └── RefreshTokenTests.cs
└── /Helpers
    ├── TestDataBuilder.cs
    └── MockDbContextFactory.cs
```

### Integration Tests

```
/tests/CarAuction.IntegrationTests/
├── CarAuction.IntegrationTests.csproj
├── GlobalUsings.cs
├── /Fixtures
│   ├── CustomWebApplicationFactory.cs
│   ├── DatabaseFixture.cs
│   └── AuthenticatedClientFixture.cs
├── /Controllers
│   ├── AuthControllerTests.cs
│   ├── CarsControllerTests.cs
│   ├── AuctionsControllerTests.cs
│   ├── BidsControllerTests.cs
│   └── UsersControllerTests.cs
├── /Hubs
│   └── AuctionHubTests.cs
└── /Helpers
    ├── HttpClientExtensions.cs
    └── TestDataSeeder.cs
```

---

## Unit Tests

### GlobalUsings.cs

```csharp
global using Xunit;
global using Moq;
global using FluentAssertions;
global using AutoMapper;
global using Microsoft.EntityFrameworkCore;
global using CarAuction.Domain.Entities;
global using CarAuction.Domain.Enums;
global using CarAuction.Domain.Exceptions;
global using CarAuction.Application.DTOs;
global using CarAuction.Application.Interfaces;
global using CarAuction.Infrastructure.Services;
global using CarAuction.Infrastructure.Data;
```

---

### Tests de Validadores

#### RegisterRequestValidatorTests.cs

```csharp
using CarAuction.Application.DTOs.Auth;
using CarAuction.Application.Validators;
using FluentValidation.TestHelper;

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
    [InlineData("password")]       // Sin mayúscula
    [InlineData("PASSWORD")]       // Sin minúscula
    [InlineData("Password")]       // Sin número
    [InlineData("Pass1")]          // Muy corta
    public void Should_HaveError_When_PasswordIsWeak(string password)
    {
        // Arrange
        var request = new RegisterRequest { Password = password };

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
            Password = "Password123",
            ConfirmPassword = "Different123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Las contraseñas no coinciden");
    }

    [Fact]
    public void Should_NotHaveErrors_When_RequestIsValid()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
            FirstName = "Juan",
            LastName = "Pérez"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

#### CreateBidRequestValidatorTests.cs

```csharp
using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.Validators;
using FluentValidation.TestHelper;

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
    public void Should_HaveError_When_AuctionIdIsInvalid(int auctionId)
    {
        // Arrange
        var request = new CreateBidRequest { AuctionId = auctionId };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AuctionId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Should_HaveError_When_AmountIsInvalid(decimal amount)
    {
        // Arrange
        var request = new CreateBidRequest { Amount = amount };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_NotHaveErrors_When_RequestIsValid()
    {
        // Arrange
        var request = new CreateBidRequest
        {
            AuctionId = 1,
            Amount = 15000
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

---

### Tests de Servicios

#### Helpers/MockDbContextFactory.cs

```csharp
using Microsoft.EntityFrameworkCore;

namespace CarAuction.UnitTests.Helpers;

public static class MockDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    public static ApplicationDbContext CreateWithData(Action<ApplicationDbContext> seedData)
    {
        var context = Create();
        seedData(context);
        context.SaveChanges();
        return context;
    }
}
```

#### Helpers/TestDataBuilder.cs

```csharp
namespace CarAuction.UnitTests.Helpers;

public static class TestDataBuilder
{
    public static User CreateUser(
        int id = 1,
        string email = "test@example.com",
        UserStatus status = UserStatus.Active)
    {
        return new User
        {
            Id = id,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            FirstName = "Test",
            LastName = "User",
            Status = status,
            EmailVerified = true
        };
    }

    public static Car CreateCar(int id = 1, string brand = "Toyota")
    {
        return new Car
        {
            Id = id,
            Brand = brand,
            Model = "Corolla",
            Year = 2022,
            Mileage = 25000,
            IsActive = true
        };
    }

    public static Auction CreateAuction(
        int id = 1,
        int carId = 1,
        AuctionStatus status = AuctionStatus.Active,
        decimal currentBid = 10000)
    {
        return new Auction
        {
            Id = id,
            CarId = carId,
            StartingPrice = 10000,
            CurrentBid = currentBid,
            MinimumBidIncrement = 100,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(7),
            Status = status
        };
    }
}
```

#### BidServiceTests.cs

```csharp
using CarAuction.Application.DTOs.Bid;
using CarAuction.UnitTests.Helpers;

namespace CarAuction.UnitTests.Services;

public class BidServiceTests
{
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    public BidServiceTests()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task PlaceBid_Should_ThrowNotFoundException_When_AuctionNotFound()
    {
        // Arrange
        var context = MockDbContextFactory.Create();
        var service = new BidService(context, _mapperMock.Object, _notificationServiceMock.Object);

        var request = new CreateBidRequest { AuctionId = 999, Amount = 15000 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.PlaceBidAsync(1, request));
    }

    [Fact]
    public async Task PlaceBid_Should_ThrowBadRequest_When_AuctionNotActive()
    {
        // Arrange
        var context = MockDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Cars.Add(TestDataBuilder.CreateCar());
            ctx.Auctions.Add(TestDataBuilder.CreateAuction(status: AuctionStatus.Completed));
        });

        var service = new BidService(context, _mapperMock.Object, _notificationServiceMock.Object);
        var request = new CreateBidRequest { AuctionId = 1, Amount = 15000 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.PlaceBidAsync(1, request));

        exception.Message.Should().Contain("no está activa");
    }

    [Fact]
    public async Task PlaceBid_Should_ThrowBadRequest_When_AmountTooLow()
    {
        // Arrange
        var context = MockDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Cars.Add(TestDataBuilder.CreateCar());
            ctx.Auctions.Add(TestDataBuilder.CreateAuction(currentBid: 15000));
        });

        var service = new BidService(context, _mapperMock.Object, _notificationServiceMock.Object);

        // CurrentBid=15000, MinIncrement=100, so minimum is 15100
        var request = new CreateBidRequest { AuctionId = 1, Amount = 15050 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.PlaceBidAsync(1, request));

        exception.Message.Should().Contain("monto mínimo");
    }

    [Fact]
    public async Task PlaceBid_Should_ThrowBadRequest_When_UserIsCurrentBidder()
    {
        // Arrange
        var context = MockDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Users.Add(TestDataBuilder.CreateUser(id: 1));
            ctx.Cars.Add(TestDataBuilder.CreateCar());
            var auction = TestDataBuilder.CreateAuction();
            auction.CurrentBidderId = 1; // User 1 is current bidder
            ctx.Auctions.Add(auction);
        });

        var service = new BidService(context, _mapperMock.Object, _notificationServiceMock.Object);
        var request = new CreateBidRequest { AuctionId = 1, Amount = 15000 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.PlaceBidAsync(1, request)); // User 1 tries to bid again

        exception.Message.Should().Contain("Ya eres el postor actual");
    }

    [Fact]
    public async Task PlaceBid_Should_CreateBid_And_UpdateAuction()
    {
        // Arrange
        var context = MockDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Users.Add(TestDataBuilder.CreateUser(id: 1));
            ctx.Users.Add(TestDataBuilder.CreateUser(id: 2, email: "other@test.com"));
            ctx.Cars.Add(TestDataBuilder.CreateCar());
            ctx.Auctions.Add(TestDataBuilder.CreateAuction(currentBid: 10000));
        });

        var service = new BidService(context, _mapperMock.Object, _notificationServiceMock.Object);
        var request = new CreateBidRequest { AuctionId = 1, Amount = 10500 };

        // Act
        var result = await service.PlaceBidAsync(userId: 2, request);

        // Assert
        result.Should().NotBeNull();
        result.NewCurrentBid.Should().Be(10500);
        result.TotalBids.Should().Be(1);

        var auction = await context.Auctions.FindAsync(1);
        auction!.CurrentBid.Should().Be(10500);
        auction.CurrentBidderId.Should().Be(2);
        auction.TotalBids.Should().Be(1);
    }

    [Fact]
    public async Task PlaceBid_Should_ExtendTime_When_NearEnd()
    {
        // Arrange
        var context = MockDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Users.Add(TestDataBuilder.CreateUser(id: 1));
            ctx.Cars.Add(TestDataBuilder.CreateCar());
            var auction = TestDataBuilder.CreateAuction();
            auction.EndTime = DateTime.UtcNow.AddMinutes(1); // 1 minute left
            auction.ExtensionThresholdMinutes = 2;
            auction.ExtensionMinutes = 5;
            ctx.Auctions.Add(auction);
        });

        var service = new BidService(context, _mapperMock.Object, _notificationServiceMock.Object);
        var request = new CreateBidRequest { AuctionId = 1, Amount = 10500 };

        // Act
        var result = await service.PlaceBidAsync(userId: 1, request);

        // Assert
        result.TimeExtended.Should().BeTrue();
        result.NewEndTime.Should().NotBeNull();
        result.NewEndTime!.Value.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(5),
            precision: TimeSpan.FromSeconds(10));
    }
}
```

#### AuthServiceTests.cs

```csharp
namespace CarAuction.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    public AuthServiceTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task RegisterAsync_Should_ThrowConflict_When_EmailExists()
    {
        // Arrange
        var context = MockDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Users.Add(TestDataBuilder.CreateUser(email: "existing@test.com"));
        });

        var service = new AuthService(context, _tokenServiceMock.Object,
            _emailServiceMock.Object, _mapperMock.Object);

        var request = new RegisterRequest
        {
            Email = "existing@test.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.RegisterAsync(request));

        exception.Message.Should().Contain("ya está registrado");
    }

    [Fact]
    public async Task LoginAsync_Should_ThrowUnauthorized_When_WrongPassword()
    {
        // Arrange
        var context = MockDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Users.Add(TestDataBuilder.CreateUser()); // Password is "Password123"
        });

        var service = new AuthService(context, _tokenServiceMock.Object,
            _emailServiceMock.Object, _mapperMock.Object);

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.LoginAsync(request));

        exception.Message.Should().Contain("Credenciales inválidas");
    }

    [Fact]
    public async Task LoginAsync_Should_ThrowUnauthorized_When_UserInactive()
    {
        // Arrange
        var context = MockDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Users.Add(TestDataBuilder.CreateUser(status: UserStatus.Pending));
        });

        var service = new AuthService(context, _tokenServiceMock.Object,
            _emailServiceMock.Object, _mapperMock.Object);

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.LoginAsync(request));

        exception.Message.Should().Contain("no está activa");
    }
}
```

---

### Tests de Entidades

#### RefreshTokenTests.cs

```csharp
namespace CarAuction.UnitTests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void IsActive_Should_ReturnTrue_When_NotRevokedAndNotExpired()
    {
        // Arrange
        var token = new RefreshToken
        {
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Act & Assert
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_Should_ReturnFalse_When_Revoked()
    {
        // Arrange
        var token = new RefreshToken
        {
            IsRevoked = true,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Act & Assert
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_Should_ReturnFalse_When_Expired()
    {
        // Arrange
        var token = new RefreshToken
        {
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        token.IsActive.Should().BeFalse();
    }
}
```

---

## Integration Tests

### Fixtures/CustomWebApplicationFactory.cs

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarAuction.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Create scope and seed database
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            SeedTestData(db);
        });
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Seed roles
        context.Roles.AddRange(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" }
        );

        // Seed test user
        var testUser = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            FirstName = "Test",
            LastName = "User",
            Status = UserStatus.Active,
            EmailVerified = true
        };
        context.Users.Add(testUser);
        context.UserRoles.Add(new UserRole { UserId = 1, RoleId = 2 });

        // Seed admin user
        var adminUser = new User
        {
            Id = 2,
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPass123"),
            FirstName = "Admin",
            LastName = "User",
            Status = UserStatus.Active,
            EmailVerified = true
        };
        context.Users.Add(adminUser);
        context.UserRoles.Add(new UserRole { UserId = 2, RoleId = 1 });

        context.SaveChanges();
    }
}
```

### Controllers/AuthControllerTests.cs

```csharp
using System.Net;
using System.Net.Http.Json;
using CarAuction.Application.DTOs.Auth;
using CarAuction.Application.DTOs.Common;
using CarAuction.IntegrationTests.Fixtures;

namespace CarAuction.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Should_ReturnSuccess_WithValidData()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        result!.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.User.Email.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task Register_Should_ReturnConflict_WhenEmailExists()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com", // Already exists in seed data
            Password = "Password123",
            ConfirmPassword = "Password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_Should_ReturnTokens_WithValidCredentials()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        result!.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_Should_ReturnUnauthorized_WithWrongPassword()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

### Helpers/HttpClientExtensions.cs

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarAuction.Application.DTOs.Auth;
using CarAuction.Application.DTOs.Common;

namespace CarAuction.IntegrationTests.Helpers;

public static class HttpClientExtensions
{
    public static async Task AuthenticateAsUserAsync(this HttpClient client)
    {
        var token = await GetTokenAsync(client, "test@example.com", "Password123");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task AuthenticateAsAdminAsync(this HttpClient client)
    {
        var token = await GetTokenAsync(client, "admin@example.com", "AdminPass123");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<string> GetTokenAsync(
        HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = email, Password = password });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return result!.Data!.AccessToken;
    }
}
```

---

## Comandos de Ejecución

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar solo unit tests
dotnet test tests/CarAuction.UnitTests

# Ejecutar solo integration tests
dotnet test tests/CarAuction.IntegrationTests

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar tests específicos
dotnet test --filter "FullyQualifiedName~BidServiceTests"

# Ejecutar tests con verbose output
dotnet test --logger "console;verbosity=detailed"

# Generar reporte de cobertura (requiere reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

---

## Métricas de Cobertura Recomendadas

| Componente | Cobertura Mínima |
|------------|------------------|
| Validators | 100% |
| Services (lógica de negocio) | 80% |
| Entities (propiedades calculadas) | 100% |
| Controllers | 70% |
| **Total proyecto** | **75%** |

---

## CI/CD Integration

### GitHub Actions (ejemplo)

```yaml
# .github/workflows/test.yml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore
      working-directory: ./backend

    - name: Build
      run: dotnet build --no-restore
      working-directory: ./backend

    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      working-directory: ./backend

    - name: Upload coverage
      uses: codecov/codecov-action@v3
```

---

## Archivos a Crear

| Archivo | Prioridad | Descripción |
|---------|-----------|-------------|
| `CarAuction.UnitTests.csproj` | Alta | Proyecto de unit tests |
| `RegisterRequestValidatorTests.cs` | Alta | Tests de validación de registro |
| `CreateBidRequestValidatorTests.cs` | Alta | Tests de validación de pujas |
| `BidServiceTests.cs` | Alta | Tests de lógica de pujas |
| `AuthServiceTests.cs` | Alta | Tests de autenticación |
| `AuctionServiceTests.cs` | Media | Tests de subastas |
| `CustomWebApplicationFactory.cs` | Media | Factory para integration tests |
| `AuthControllerTests.cs` | Media | Tests de endpoints de auth |
