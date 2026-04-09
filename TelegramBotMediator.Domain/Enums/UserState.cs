namespace TelegramBotMediator.Domain.Enums;

public enum UserState
{
    None = 0,
    WaitingForFirstName = 1,
    WaitingForLastName = 2,
    WaitingForPhone = 3,
    WaitingForAddress = 4,
    Registered = 5
}
