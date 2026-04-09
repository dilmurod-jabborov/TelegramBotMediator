using TelegramBotMediator.Application.Dtos;
using TelegramBotMediator.Domain.Enums;

namespace TelegramBotMediator.Application.Abstractions.Services;

public interface IStateService
{
    UserState GetState(long telegramId);
    void SetState(long telegramId, UserState state);
    UserRegistrationData GetOrCreateRegistrationData(long telegramId);
    void ClearRegistration(long telegramId);
    void TrackMessage(long telegramId, int messageId);
    IReadOnlyCollection<int> GetTrackedMessages(long telegramId);
    void ClearTrackedMessages(long telegramId);
}
