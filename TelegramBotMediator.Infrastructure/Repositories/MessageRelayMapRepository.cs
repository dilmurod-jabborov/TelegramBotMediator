using Microsoft.EntityFrameworkCore;
using TelegramBotMediator.Application.Abstractions.Repositories;
using TelegramBotMediator.Domain.Entities;
using TelegramBotMediator.Infrastructure.Data;

namespace TelegramBotMediator.Infrastructure.Repositories;

public sealed class MessageRelayMapRepository(AppDbContext dbContext) : IMessageRelayMapRepository
{
    public async Task AddAsync(MessageRelayMap map, CancellationToken cancellationToken = default)
    {
        await dbContext.MessageRelayMaps.AddAsync(map, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<MessageRelayMap?> GetByForwardedMessageIdAsync(int forwardedMessageId, CancellationToken cancellationToken = default)
    {
        return dbContext.MessageRelayMaps.FirstOrDefaultAsync(x => x.ForwardedMessageId == forwardedMessageId, cancellationToken);
    }

    public async Task<int> DeleteOlderThanAsync(DateTime olderThanUtc, CancellationToken cancellationToken = default)
    {
        var oldMaps = await dbContext.MessageRelayMaps
            .Where(x => x.CreatedAt < olderThanUtc)
            .ToListAsync(cancellationToken);

        if (oldMaps.Count == 0)
        {
            return 0;
        }

        dbContext.MessageRelayMaps.RemoveRange(oldMaps);
        await dbContext.SaveChangesAsync(cancellationToken);
        return oldMaps.Count;
    }
}
