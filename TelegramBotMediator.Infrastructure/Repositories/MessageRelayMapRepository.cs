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
}
