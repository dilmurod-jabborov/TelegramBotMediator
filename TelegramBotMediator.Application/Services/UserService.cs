using TelegramBotMediator.Application.Abstractions.Repositories;
using TelegramBotMediator.Application.Abstractions.Services;
using TelegramBotMediator.Domain.Entities;

namespace TelegramBotMediator.Application.Services;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
    public Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return userRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
    }

    public async Task<User> RegisterAsync(long telegramId, string firstName, string lastName, string phoneNumber, string address, CancellationToken cancellationToken = default)
    {
        var existing = await userRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var user = new User
        {
            TelegramId = telegramId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            Address = address.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);
        return user;
    }

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return userRepository.GetAllAsync(cancellationToken);
    }
}
