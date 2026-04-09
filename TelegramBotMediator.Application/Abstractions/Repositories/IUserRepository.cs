using TelegramBotMediator.Domain.Entities;

namespace TelegramBotMediator.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
