using TelegramBotMediator.Domain.Entities;

namespace TelegramBotMediator.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTodayCountAsync(CancellationToken cancellationToken = default);
    Task<bool> SetBanStatusAsync(long telegramId, bool isBanned, CancellationToken cancellationToken = default);
}
