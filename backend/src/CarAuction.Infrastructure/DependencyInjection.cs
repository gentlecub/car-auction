using CarAuction.Application.Interfaces;
using CarAuction.Infrastructure.Data;
using CarAuction.Infrastructure.Services;
using CarAuction.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarAuction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Settings
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));


        // Redis Cache
        var redisConnection = configuration["Redis:ConnectionString"];
        var redisInstanceName = configuration["Redis:InstanceName"] ?? "CarAuction_";

        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = redisInstanceName;
            });
        }
        else
        {
            // Fallback to in-memory cache for development
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, RedisCacheService>();

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICarService, CarService>();
        services.AddScoped<IAuctionService, AuctionService>();
        services.AddScoped<IBidService, BidService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFileUploadService, LocalFileUploadService>();

        return services;
    }
}
