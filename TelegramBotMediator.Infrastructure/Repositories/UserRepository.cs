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

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Users.OrderBy(x => x.Id).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
