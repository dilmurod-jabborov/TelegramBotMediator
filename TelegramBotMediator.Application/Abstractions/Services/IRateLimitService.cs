namespace TelegramBotMediator.Application.Abstractions.Services;

public interface IRateLimitService
{
    bool IsAllowed(long telegramId, TimeSpan minInterval);
}
