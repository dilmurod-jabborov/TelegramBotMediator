using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramBotMediator.Presentation.Configuration;

namespace TelegramBotMediator.Presentation.Services;

public sealed class WebhookSetupService(
    ITelegramBotClient botClient,
    BotSettings botSettings,
    ILogger<WebhookSetupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!botSettings.UseWebhook || string.IsNullOrWhiteSpace(botSettings.WebhookUrl))
        {
            return;
        }

        await botClient.DeleteWebhook(dropPendingUpdates: false, cancellationToken: cancellationToken);
        await botClient.SetWebhook(
            url: $"{botSettings.WebhookUrl.TrimEnd('/')}{botSettings.WebhookPath}",
            cancellationToken: cancellationToken);

        logger.LogInformation("Webhook configured at {WebhookUrl}{WebhookPath}.", botSettings.WebhookUrl, botSettings.WebhookPath);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
