using TelegramBotMediator.Domain.Entities;

namespace TelegramBotMediator.Application.Abstractions.Repositories;

public interface IMessageRelayMapRepository
{
    Task AddAsync(MessageRelayMap map, CancellationToken cancellationToken = default);
    Task<MessageRelayMap?> GetByForwardedMessageIdAsync(int forwardedMessageId, CancellationToken cancellationToken = default);
}
