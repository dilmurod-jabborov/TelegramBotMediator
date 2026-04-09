using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotMediator.Application.Abstractions.Repositories;

namespace TelegramBotMediator.Presentation.Services;

public sealed class RelayCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<RelayCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IMessageRelayMapRepository>();
                var deleted = await repository.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30), stoppingToken);
                if (deleted > 0)
                {
                    logger.LogInformation("Relay cleanup removed {Count} old mappings.", deleted);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Relay cleanup job failed.");
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
