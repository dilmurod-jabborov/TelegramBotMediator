using Microsoft.Extensions.DependencyInjection;
using TelegramBotMediator.Application.Abstractions.Services;
using TelegramBotMediator.Application.Services;

namespace TelegramBotMediator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IStateService, StateService>();
        services.AddSingleton<IRateLimitService, RateLimitService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
