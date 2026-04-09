namespace TelegramBotMediator.Domain.Entities;

public sealed class UserSession
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public int State { get; set; }
    public string FirstNameDraft { get; set; } = string.Empty;
    public string LastNameDraft { get; set; } = string.Empty;
    public string PhoneDraft { get; set; } = string.Empty;
    public string AddressDraft { get; set; } = string.Empty;
    public string TrackedMessageIdsJson { get; set; } = "[]";
    public DateTime UpdatedAt { get; set; }
}
