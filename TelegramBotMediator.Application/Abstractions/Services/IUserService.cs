using TelegramBotMediator.Domain.Entities;

namespace TelegramBotMediator.Application.Abstractions.Services;

public interface IUserService
{
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User> RegisterAsync(long telegramId, string firstName, string lastName, string phoneNumber, string address, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTodayCountAsync(CancellationToken cancellationToken = default);
    Task<bool> BanAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<bool> UnbanAsync(long telegramId, CancellationToken cancellationToken = default);
}
