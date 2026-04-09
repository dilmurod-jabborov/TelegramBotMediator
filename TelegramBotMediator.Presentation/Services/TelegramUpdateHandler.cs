using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.Logging;
using TelegramBotMediator.Application.Abstractions.Services;
using TelegramBotMediator.Domain.Enums;

namespace TelegramBotMediator.Presentation.Services;

public sealed class TelegramUpdateHandler(
    ILogger<TelegramUpdateHandler> logger,
    IStateService stateService,
    IUserService userService,
    IMessageRelayService relayService,
    long adminTelegramId) : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message is null || update.Message.From is null)
        {
            return;
        }

        var message = update.Message;
        var telegramId = message.From.Id;
        var isAdmin = telegramId == adminTelegramId;

        if (isAdmin)
        {
            await HandleAdminMessageAsync(botClient, message, cancellationToken);
            return;
        }

        await HandleUserMessageAsync(botClient, message, telegramId, cancellationToken);
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Telegram update xatoligi. Manba: {Source}", source);
        return Task.CompletedTask;
    }

    private async Task HandleAdminMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.Text is "/users")
        {
            var users = await userService.GetAllAsync(cancellationToken);
            var text = users.Count == 0
                ? "Hozircha foydalanuvchilar yo'q."
                : string.Join(Environment.NewLine, users.Select(u => $"{u.Id}. {u.FirstName} {u.LastName} - {u.PhoneNumber}"));
            await botClient.SendMessage(adminTelegramId, text, cancellationToken: cancellationToken);
            return;
        }

        if (!string.IsNullOrWhiteSpace(message.Text) && message.Text.StartsWith("/broadcast ", StringComparison.OrdinalIgnoreCase))
        {
            var broadcastText = message.Text["/broadcast ".Length..].Trim();
            if (string.IsNullOrWhiteSpace(broadcastText))
            {
                await botClient.SendMessage(adminTelegramId, "Foydalanish: /broadcast <xabar>", cancellationToken: cancellationToken);
                return;
            }

            await relayService.BroadcastAsync(broadcastText, cancellationToken);
            await botClient.SendMessage(adminTelegramId, "Xabar barcha foydalanuvchilarga yuborildi.", cancellationToken: cancellationToken);
            return;
        }

        if (message.ReplyToMessage is null || string.IsNullOrWhiteSpace(message.Text))
        {
            await botClient.SendMessage(adminTelegramId, "Javob berish uchun foydalanuvchi xabariga reply qiling.", cancellationToken: cancellationToken);
            return;
        }

        var ok = await relayService.RelayAdminReplyToUserAsync(message.ReplyToMessage.MessageId, message.Text, cancellationToken);
        var statusText = ok ? "Javob foydalanuvchiga yuborildi." : "Bu reply uchun foydalanuvchi aniqlanmadi.";
        await botClient.SendMessage(adminTelegramId, statusText, cancellationToken: cancellationToken);
    }

    private async Task HandleUserMessageAsync(ITelegramBotClient botClient, Message message, long telegramId, CancellationToken cancellationToken)
    {
        if (message.Text == "/start")
        {
            var existing = await userService.GetByTelegramIdAsync(telegramId, cancellationToken);
            if (existing is not null)
            {
                stateService.SetState(telegramId, UserState.Registered);
                await botClient.SendMessage(message.Chat.Id, "Siz allaqachon ro'yxatdan o'tgansiz. Qanday savolingiz bor?", cancellationToken: cancellationToken);
                return;
            }

            stateService.ClearRegistration(telegramId);
            stateService.SetState(telegramId, UserState.WaitingForFirstName);
            await botClient.SendMessage(message.Chat.Id, "Ismingizni kiriting:", cancellationToken: cancellationToken);
            return;
        }

        var existingUser = await userService.GetByTelegramIdAsync(telegramId, cancellationToken);
        var currentState = stateService.GetState(telegramId);

        if (existingUser is not null && currentState != UserState.Registered)
        {
            stateService.SetState(telegramId, UserState.Registered);
            currentState = UserState.Registered;
        }

        if (currentState == UserState.None && existingUser is null)
        {
            await botClient.SendMessage(message.Chat.Id, "Avval /start buyrug'i orqali ro'yxatdan o'ting.", cancellationToken: cancellationToken);
            return;
        }

        if (currentState == UserState.Registered)
        {
            var userMessageText = message.Text ?? message.Caption ?? "[Matnsiz xabar]";
            await relayService.RelayUserMessageToAdminAsync(telegramId, userMessageText, cancellationToken);
            return;
        }

        var registration = stateService.GetOrCreateRegistrationData(telegramId);

        switch (currentState)
        {
            case UserState.WaitingForFirstName:
                if (string.IsNullOrWhiteSpace(message.Text))
                {
                    await botClient.SendMessage(message.Chat.Id, "Ism bo'sh bo'lmasligi kerak. Ismingizni kiriting:", cancellationToken: cancellationToken);
                    return;
                }

                registration.FirstName = message.Text.Trim();
                stateService.SetState(telegramId, UserState.WaitingForLastName);
                await botClient.SendMessage(message.Chat.Id, "Familiyangizni kiriting:", cancellationToken: cancellationToken);
                break;

            case UserState.WaitingForLastName:
                if (string.IsNullOrWhiteSpace(message.Text))
                {
                    await botClient.SendMessage(message.Chat.Id, "Familiya bo'sh bo'lmasligi kerak. Familiyangizni kiriting:", cancellationToken: cancellationToken);
                    return;
                }

                registration.LastName = message.Text.Trim();
                stateService.SetState(telegramId, UserState.WaitingForPhone);

                var contactButton = KeyboardButton.WithRequestContact("Telefon raqamni yuborish");
                var keyboard = new ReplyKeyboardMarkup(contactButton)
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true
                };

                await botClient.SendMessage(message.Chat.Id, "Telefon raqamingizni yuboring:", replyMarkup: keyboard, cancellationToken: cancellationToken);
                break;

            case UserState.WaitingForPhone:
                if (message.Contact is null
                    || string.IsNullOrWhiteSpace(message.Contact.PhoneNumber)
                    || message.Contact.UserId != telegramId)
                {
                    await botClient.SendMessage(message.Chat.Id, "Iltimos, tugma orqali o'zingizning telefon raqamingizni yuboring.", cancellationToken: cancellationToken);
                    return;
                }

                registration.PhoneNumber = message.Contact.PhoneNumber.Trim();
                stateService.SetState(telegramId, UserState.WaitingForAddress);
                await botClient.SendMessage(message.Chat.Id, "Manzilingizni kiriting:", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
                break;

            case UserState.WaitingForAddress:
                if (string.IsNullOrWhiteSpace(message.Text))
                {
                    await botClient.SendMessage(message.Chat.Id, "Manzil bo'sh bo'lmasligi kerak. Manzilingizni kiriting:", cancellationToken: cancellationToken);
                    return;
                }

                registration.Address = message.Text.Trim();
                await userService.RegisterAsync(telegramId, registration.FirstName, registration.LastName, registration.PhoneNumber, registration.Address, cancellationToken);
                stateService.SetState(telegramId, UserState.Registered);
                stateService.ClearRegistration(telegramId);
                stateService.SetState(telegramId, UserState.Registered);

                await botClient.SendMessage(message.Chat.Id, "Siz muvaffaqiyatli ro'yxatdan o'tdingiz. Qanday savolingiz bor?", cancellationToken: cancellationToken);
                break;

            default:
                logger.LogWarning("Missing or unknown state for user {TelegramId}", telegramId);
                await botClient.SendMessage(message.Chat.Id, "Holat aniqlanmadi. Iltimos, /start buyrug'ini qayta yuboring.", cancellationToken: cancellationToken);
                stateService.ClearRegistration(telegramId);
                break;
        }
    }
}
