namespace TelegramBotMediator.Domain.Entities;

public sealed class User
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsBanned { get; set; }
    public DateTime CreatedAt { get; set; }
}
