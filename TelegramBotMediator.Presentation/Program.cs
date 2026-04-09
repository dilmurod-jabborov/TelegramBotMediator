using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramBotMediator.Application;
using TelegramBotMediator.Application.Abstractions.Repositories;
using TelegramBotMediator.Application.Abstractions.Services;
using TelegramBotMediator.Application.Services;
using TelegramBotMediator.Infrastructure;
using TelegramBotMediator.Infrastructure.Data;
using TelegramBotMediator.Presentation.Configuration;
using TelegramBotMediator.Presentation.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<BotSettings>(builder.Configuration.GetSection(BotSettings.SectionName));
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
    sp.GetRequiredService<IUserService>(),
    sp.GetRequiredService<IMessageRelayService>(),
    botSettings.AdminTelegramId));
builder.Services.AddHostedService<BotPollingService>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsNpgsql())
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }
}

await host.RunAsync();
