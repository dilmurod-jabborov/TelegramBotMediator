namespace TelegramBotMediator.Presentation.Configuration;

public sealed class BotSettings
{
    public const string SectionName = "BotSettings";

    public string Token { get; set; } = string.Empty;
    public long AdminTelegramId { get; set; }
    public bool UseWebhook { get; set; }
    public string WebhookUrl { get; set; } = string.Empty;
    public string WebhookPath { get; set; } = "/telegram/webhook";
    public int UserMessageRateLimitMs { get; set; } = 800;
}
