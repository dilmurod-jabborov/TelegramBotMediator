using Microsoft.EntityFrameworkCore;
using TelegramBotMediator.Application.Abstractions.Repositories;
using TelegramBotMediator.Domain.Entities;
using TelegramBotMediator.Infrastructure.Data;

namespace TelegramBotMediator.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.FirstOrDefaultAsync(x => x.TelegramId == telegramId, cancellationToken);
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Users.OrderBy(x => x.Id).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Users.CountAsync(cancellationToken);
    }

    public Task<int> GetTodayCountAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return dbContext.Users.CountAsync(x => x.CreatedAt >= today && x.CreatedAt < tomorrow, cancellationToken);
    }

    public async Task<bool> SetBanStatusAsync(long telegramId, bool isBanned, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.TelegramId == telegramId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.IsBanned = isBanned;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
