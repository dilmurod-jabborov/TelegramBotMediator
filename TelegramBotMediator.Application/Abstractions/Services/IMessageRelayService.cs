namespace TelegramBotMediator.Application.Abstractions.Services;

public interface IMessageRelayService
{
    Task<int> RelayUserMessageToAdminAsync(long userTelegramId, string userMessage, CancellationToken cancellationToken = default);
    Task<bool> RelayAdminReplyToUserAsync(int replyToMessageId, string adminMessage, CancellationToken cancellationToken = default);
    Task BroadcastAsync(string message, CancellationToken cancellationToken = default);
}
