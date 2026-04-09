using System.Collections.Concurrent;
using TelegramBotMediator.Application.Abstractions.Services;
using TelegramBotMediator.Application.Dtos;
using TelegramBotMediator.Domain.Enums;

namespace TelegramBotMediator.Application.Services;

public sealed class StateService : IStateService
{
    private readonly ConcurrentDictionary<long, UserState> _states = new();
    private readonly ConcurrentDictionary<long, UserRegistrationData> _registrations = new();
    private readonly ConcurrentDictionary<long, ConcurrentQueue<int>> _trackedMessages = new();

    public UserState GetState(long telegramId)
    {
        return _states.GetValueOrDefault(telegramId, UserState.None);
    }

    public void SetState(long telegramId, UserState state)
    {
        _states[telegramId] = state;
    }

    public UserRegistrationData GetOrCreateRegistrationData(long telegramId)
    {
        return _registrations.GetOrAdd(telegramId, _ => new UserRegistrationData());
    }

    public void ClearRegistration(long telegramId)
    {
        _registrations.TryRemove(telegramId, out _);
        _states.TryRemove(telegramId, out _);
    }

    public void TrackMessage(long telegramId, int messageId)
    {
        var queue = _trackedMessages.GetOrAdd(telegramId, _ => new ConcurrentQueue<int>());
        queue.Enqueue(messageId);

        // Keep bounded history to avoid unbounded memory growth.
        while (queue.Count > 100 && queue.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyCollection<int> GetTrackedMessages(long telegramId)
    {
        if (!_trackedMessages.TryGetValue(telegramId, out var queue))
        {
            return [];
        }

        return queue.ToArray();
    }

    public void ClearTrackedMessages(long telegramId)
    {
        _trackedMessages.TryRemove(telegramId, out _);
    }
}
