using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace CarAuction.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// Replaces the database with an in-memory database and seeds test data
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid());
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create scope and initialize database
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureCreated();

            // Seed test data
            SeedTestData(db);
        });

        builder.UseEnvironment("Testing");
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Seed roles
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Administrador del sistema"
                },
                new Role
                {
                    Id = 2,
                    Name = "User",
                    Description = "Usuario estándar"
                }
            );
        }

        // Seed test admin user
        // Password: Admin123!
        if (!context.Users.Any(u => u.Email == "admin@test.com"))
        {
            var adminUser = new User
            {
                Id = 1,
                Email = "admin@test.com",
                PasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.4HqoQz4L4XK/Oi",
                FirstName = "Admin",
                LastName = "Test",
                Status = UserStatus.Active,
                EmailVerified = true
            };
            context.Users.Add(adminUser);
        }

        // Seed test regular user
        // Password: User123!
        if (!context.Users.Any(u => u.Email == "user@test.com"))
        {
            var testUser = new User
            {
                Id = 2,
                Email = "user@test.com",
                PasswordHash = "$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi",
                FirstName = "Test",
                LastName = "User",
                Status = UserStatus.Active,
                EmailVerified = true
            };
            context.Users.Add(testUser);
        }

        context.SaveChanges();

        // Assign roles
        if (!context.UserRoles.Any())
        {
            context.UserRoles.AddRange(
                new UserRole { UserId = 1, RoleId = 1 }, // Admin role
                new UserRole { UserId = 2, RoleId = 2 }  // User role
            );
            context.SaveChanges();
        }

        // Seed test car
        if (!context.Cars.Any())
        {
            var car = new Car
            {
                Id = 1,
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2022,
                Mileage = 25000,
                Color = "Blanco",
                IsActive = true
            };
            context.Cars.Add(car);
            context.SaveChanges();
        }

        // Seed test auction
        if (!context.Auctions.Any())
        {
            var auction = new Auction
            {
                Id = 1,
                CarId = 1,
                StartingPrice = 10000,
                CurrentBid = 10000,
                MinimumBidIncrement = 100,
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(7),
                Status = AuctionStatus.Active
            };
            context.Auctions.Add(auction);
            context.SaveChanges();
        }
    }
}
