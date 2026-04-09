using TelegramBotMediator.Application.Dtos;
using TelegramBotMediator.Domain.Enums;

namespace TelegramBotMediator.Application.Abstractions.Services;

public interface IStateService
{
    Task<UserState> GetStateAsync(long telegramId, CancellationToken cancellationToken = default);
    Task SetStateAsync(long telegramId, UserState state, CancellationToken cancellationToken = default);
    Task<UserRegistrationData> GetOrCreateRegistrationDataAsync(long telegramId, CancellationToken cancellationToken = default);
    Task SaveRegistrationDataAsync(long telegramId, UserRegistrationData registrationData, CancellationToken cancellationToken = default);
    Task ClearRegistrationAsync(long telegramId, CancellationToken cancellationToken = default);
    Task TrackMessageAsync(long telegramId, int messageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>> GetTrackedMessagesAsync(long telegramId, CancellationToken cancellationToken = default);
    Task ClearTrackedMessagesAsync(long telegramId, CancellationToken cancellationToken = default);
}
