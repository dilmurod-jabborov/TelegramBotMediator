namespace TelegramBotMediator.Domain.Entities;

public sealed class MessageRelayMap
{
    public int Id { get; set; }
    public int ForwardedMessageId { get; set; }
    public long UserTelegramId { get; set; }
    public DateTime CreatedAt { get; set; }
}
