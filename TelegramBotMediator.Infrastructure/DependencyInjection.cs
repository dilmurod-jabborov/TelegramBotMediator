using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotMediator.Application.Abstractions.Repositories;
using TelegramBotMediator.Infrastructure.Data;
using TelegramBotMediator.Infrastructure.Repositories;

namespace TelegramBotMediator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=telegram-bot.db";
        var databaseUrl = configuration["DATABASE_URL"];

        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection"))
            && !string.IsNullOrWhiteSpace(databaseUrl))
        {
            connectionString = databaseUrl;
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            if (IsPostgresConnectionString(connectionString))
            {
                var normalized = NormalizePostgresConnectionString(connectionString);
                options.UseNpgsql(normalized);
                return;
            }

            options.UseSqlite(connectionString);
        });
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMessageRelayMapRepository, MessageRelayMapRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();

        return services;
    }

    private static bool IsPostgresConnectionString(string connectionString)
    {
        return connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
               || connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase)
               || connectionString.Contains("Port=", StringComparison.OrdinalIgnoreCase)
               || connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase)
               || connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
               || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePostgresConnectionString(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty);
        var password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty);
        var database = uri.AbsolutePath.Trim('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
