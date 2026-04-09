using TelegramBotMediator.Domain.Entities;

namespace TelegramBotMediator.Application.Abstractions.Services;

public interface IUserService
{
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<User> RegisterAsync(long telegramId, string firstName, string lastName, string phoneNumber, string address, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
}
