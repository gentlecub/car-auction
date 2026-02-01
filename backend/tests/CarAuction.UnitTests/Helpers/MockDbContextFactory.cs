namespace CarAuction.UnitTests.Helpers;

/// <summary>
/// Factory for creating in-memory database contexts for unit testing
/// </summary>
public static class MockDbContextFactory
{
    /// <summary>
    /// Creates a new in-memory database context with a unique database name
    /// </summary>
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Creates a new in-memory database context with seed data
    /// </summary>
    /// <param name="seedData">Action to seed data into the context</param>
    public static ApplicationDbContext CreateWithData(Action<ApplicationDbContext> seedData)
    {
        var context = Create();
        seedData(context);
        context.SaveChanges();
        return context;
    }

    /// <summary>
    /// Creates a context with default roles seeded
    /// </summary>
    public static ApplicationDbContext CreateWithRoles()
    {
        return CreateWithData(context =>
        {
            context.Roles.AddRange(
                new Role { Id = 1, Name = "Admin", Description = "Administrador" },
                new Role { Id = 2, Name = "User", Description = "Usuario" }
            );
        });
    }
}
