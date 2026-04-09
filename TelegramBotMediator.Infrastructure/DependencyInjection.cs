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

        services.AddDbContext<AppDbContext>(options =>
        {
            if (IsPostgresConnectionString(connectionString))
            {
                options.UseNpgsql(connectionString);
                return;
            }

            options.UseSqlite(connectionString);
        });
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMessageRelayMapRepository, MessageRelayMapRepository>();

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
}
