using System.Text.Json;
using TelegramBotMediator.Application.Abstractions.Repositories;
using TelegramBotMediator.Application.Abstractions.Services;
using TelegramBotMediator.Application.Dtos;
using TelegramBotMediator.Domain.Entities;
using TelegramBotMediator.Domain.Enums;

namespace TelegramBotMediator.Application.Services;

public sealed class StateService(IUserSessionRepository userSessionRepository) : IStateService
{
    public async Task<UserState> GetStateAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var session = await userSessionRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        return session is null ? UserState.None : (UserState)session.State;
    }

    public async Task SetStateAsync(long telegramId, UserState state, CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(telegramId, cancellationToken);
        session.State = (int)state;
        session.UpdatedAt = DateTime.UtcNow;
        await userSessionRepository.SaveAsync(session, cancellationToken);
    }

    public async Task<UserRegistrationData> GetOrCreateRegistrationDataAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(telegramId, cancellationToken);
        return new UserRegistrationData
        {
            FirstName = session.FirstNameDraft,
            LastName = session.LastNameDraft,
            PhoneNumber = session.PhoneDraft,
            Address = session.AddressDraft
        };
    }

    public async Task SaveRegistrationDataAsync(long telegramId, UserRegistrationData registrationData, CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(telegramId, cancellationToken);
        session.FirstNameDraft = registrationData.FirstName;
        session.LastNameDraft = registrationData.LastName;
        session.PhoneDraft = registrationData.PhoneNumber;
        session.AddressDraft = registrationData.Address;
        session.UpdatedAt = DateTime.UtcNow;
        await userSessionRepository.SaveAsync(session, cancellationToken);
    }

    public async Task ClearRegistrationAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        await userSessionRepository.DeleteAsync(telegramId, cancellationToken);
    }

    public async Task TrackMessageAsync(long telegramId, int messageId, CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(telegramId, cancellationToken);
        var ids = ParseTrackedIds(session.TrackedMessageIdsJson);
        ids.Add(messageId);
        if (ids.Count > 100)
        {
            ids = ids.TakeLast(100).ToList();
        }

        session.TrackedMessageIdsJson = JsonSerializer.Serialize(ids);
        session.UpdatedAt = DateTime.UtcNow;
        await userSessionRepository.SaveAsync(session, cancellationToken);
    }

    public async Task<IReadOnlyCollection<int>> GetTrackedMessagesAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var session = await userSessionRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        if (session is null)
        {
            return [];
        }

        return ParseTrackedIds(session.TrackedMessageIdsJson);
    }

    public async Task ClearTrackedMessagesAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var session = await userSessionRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        if (session is null)
        {
            return;
        }

        session.TrackedMessageIdsJson = "[]";
        session.UpdatedAt = DateTime.UtcNow;
        await userSessionRepository.SaveAsync(session, cancellationToken);
    }

    private async Task<UserSession> GetOrCreateSessionAsync(long telegramId, CancellationToken cancellationToken)
    {
        var session = await userSessionRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        if (session is not null)
        {
            return session;
        }

        return new UserSession
        {
            TelegramId = telegramId,
            State = (int)UserState.None,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static List<int> ParseTrackedIds(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<int>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
