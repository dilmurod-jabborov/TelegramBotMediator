using System.Collections.Concurrent;
using TelegramBotMediator.Application.Abstractions.Services;

namespace TelegramBotMediator.Application.Services;

public sealed class RateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<long, DateTime> _lastSeen = new();

    public bool IsAllowed(long telegramId, TimeSpan minInterval)
    {
        var now = DateTime.UtcNow;
        var last = _lastSeen.GetValueOrDefault(telegramId, DateTime.MinValue);
        if (now - last < minInterval)
        {
            return false;
        }

        _lastSeen[telegramId] = now;
        return true;
    }
}
