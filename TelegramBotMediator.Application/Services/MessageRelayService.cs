using Polly;
using Telegram.Bot;
using TelegramBotMediator.Application.Abstractions.Repositories;
using TelegramBotMediator.Application.Abstractions.Services;
using TelegramBotMediator.Domain.Entities;

namespace TelegramBotMediator.Application.Services;

public sealed class MessageRelayService(
    ITelegramBotClient botClient,
    IUserRepository userRepository,
    IMessageRelayMapRepository relayMapRepository,
    IUserService userService,
    long adminTelegramId) : IMessageRelayService
{
    private readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(300),
            BackoffType = DelayBackoffType.Exponential
        })
        .Build();

    public async Task<int> RelayUserMessageToAdminAsync(long userTelegramId, string userMessage, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByTelegramIdAsync(userTelegramId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User is not registered.");
        }

        var relayText = $"""
                         User: {user.FirstName} {user.LastName}
                         Phone: {user.PhoneNumber}
                         Address: {user.Address}

                         Message:
                         {userMessage}
                         """;

        var sent = await _pipeline.ExecuteAsync(
            async token => await botClient.SendMessage(adminTelegramId, relayText, cancellationToken: token),
            cancellationToken);
        await relayMapRepository.AddAsync(new MessageRelayMap
        {
            ForwardedMessageId = sent.MessageId,
            UserTelegramId = userTelegramId,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        return sent.MessageId;
    }

    public async Task<bool> RelayAdminReplyToUserAsync(int replyToMessageId, string adminMessage, CancellationToken cancellationToken = default)
    {
        var map = await relayMapRepository.GetByForwardedMessageIdAsync(replyToMessageId, cancellationToken);
        if (map is null)
        {
            return false;
        }

        await _pipeline.ExecuteAsync(
            async token => await botClient.SendMessage(map.UserTelegramId, adminMessage, cancellationToken: token),
            cancellationToken);
        return true;
    }

    public async Task BroadcastAsync(string message, CancellationToken cancellationToken = default)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        foreach (var user in users)
        {
            if (user.IsBanned)
            {
                continue;
            }

            await _pipeline.ExecuteAsync(
                async token => await botClient.SendMessage(user.TelegramId, message, cancellationToken: token),
                cancellationToken);
        }
    }
}
