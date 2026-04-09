using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TelegramBotMediator.Presentation.Services;

public sealed class BotPollingService(
    ITelegramBotClient botClient,
    IServiceScopeFactory scopeFactory,
    ILogger<BotPollingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await botClient.GetMe(stoppingToken);
        logger.LogInformation("Bot started: {Username} ({Id})", me.Username, me.Id);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        using var scope = scopeFactory.CreateScope();
        var updateHandler = scope.ServiceProvider.GetRequiredService<TelegramUpdateHandler>();
        botClient.StartReceiving(updateHandler, receiverOptions, cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Bot polling cancelled.");
        }
    }
}
