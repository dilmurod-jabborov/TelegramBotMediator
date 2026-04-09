using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotMediator.Application;
using TelegramBotMediator.Application.Abstractions.Repositories;
using TelegramBotMediator.Application.Abstractions.Services;
using TelegramBotMediator.Application.Services;
using TelegramBotMediator.Infrastructure;
using TelegramBotMediator.Infrastructure.Data;
using TelegramBotMediator.Presentation.Configuration;
using TelegramBotMediator.Presentation.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddLogging(logging => logging.AddSerilog(Log.Logger, dispose: true));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var botSettings = builder.Configuration.GetSection(BotSettings.SectionName).Get<BotSettings>()
                 ?? throw new InvalidOperationException("BotSettings section is missing.");

if (string.IsNullOrWhiteSpace(botSettings.Token))
{
    throw new InvalidOperationException("Bot token is missing in appsettings.json");
}

builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botSettings.Token));
builder.Services.AddScoped<IMessageRelayService>(sp => new MessageRelayService(
    sp.GetRequiredService<ITelegramBotClient>(),
    sp.GetRequiredService<IUserRepository>(),
    sp.GetRequiredService<IMessageRelayMapRepository>(),
    sp.GetRequiredService<IUserService>(),
    botSettings.AdminTelegramId));
builder.Services.AddScoped<TelegramUpdateHandler>(sp => new TelegramUpdateHandler(
    sp.GetRequiredService<ILogger<TelegramUpdateHandler>>(),
    sp.GetRequiredService<IStateService>(),
    sp.GetRequiredService<IRateLimitService>(),
    sp.GetRequiredService<IUserService>(),
    sp.GetRequiredService<IMessageRelayService>(),
    botSettings.AdminTelegramId,
    botSettings.UserMessageRateLimitMs));

if (botSettings.UseWebhook)
{
    builder.Services.AddSingleton(botSettings);
    builder.Services.AddHostedService<WebhookSetupService>();
}
else
{
    builder.Services.AddHostedService<BotPollingService>();
}

builder.Services.AddHostedService<RelayCleanupService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (botSettings.UseWebhook)
{
    var webhookPath = botSettings.WebhookPath.StartsWith('/') ? botSettings.WebhookPath : $"/{botSettings.WebhookPath}";
    app.MapPost(webhookPath, async (Update update, TelegramUpdateHandler updateHandler, ITelegramBotClient botClient, CancellationToken ct) =>
    {
        await updateHandler.HandleUpdateAsync(botClient, update, ct);
        return Results.Ok();
    });
}

await app.RunAsync();
