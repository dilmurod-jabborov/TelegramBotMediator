using TelegramBotMediator.Domain.Entities;

namespace TelegramBotMediator.Application.Abstractions.Repositories;

public interface IUserSessionRepository
{
    Task<UserSession?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
    Task SaveAsync(UserSession session, CancellationToken cancellationToken = default);
    Task DeleteAsync(long telegramId, CancellationToken cancellationToken = default);
}
