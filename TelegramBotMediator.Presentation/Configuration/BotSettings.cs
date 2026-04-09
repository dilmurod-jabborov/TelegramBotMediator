namespace TelegramBotMediator.Presentation.Configuration;

public sealed class BotSettings
{
    public const string SectionName = "BotSettings";

    public string Token { get; set; } = string.Empty;
    public long AdminTelegramId { get; set; }
}
