using Microsoft.EntityFrameworkCore;
using TelegramBotMediator.Application.Abstractions.Repositories;
using TelegramBotMediator.Domain.Entities;
using TelegramBotMediator.Infrastructure.Data;

namespace TelegramBotMediator.Infrastructure.Repositories;

public sealed class UserSessionRepository(AppDbContext dbContext) : IUserSessionRepository
{
    public Task<UserSession?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return dbContext.UserSessions.FirstOrDefaultAsync(x => x.TelegramId == telegramId, cancellationToken);
    }

    public async Task SaveAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.UserSessions.FirstOrDefaultAsync(x => x.TelegramId == session.TelegramId, cancellationToken);
        if (existing is null)
        {
            await dbContext.UserSessions.AddAsync(session, cancellationToken);
        }
        else
        {
            existing.State = session.State;
            existing.FirstNameDraft = session.FirstNameDraft;
            existing.LastNameDraft = session.LastNameDraft;
            existing.PhoneDraft = session.PhoneDraft;
            existing.AddressDraft = session.AddressDraft;
            existing.TrackedMessageIdsJson = session.TrackedMessageIdsJson;
            existing.UpdatedAt = session.UpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.UserSessions.FirstOrDefaultAsync(x => x.TelegramId == telegramId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        dbContext.UserSessions.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
