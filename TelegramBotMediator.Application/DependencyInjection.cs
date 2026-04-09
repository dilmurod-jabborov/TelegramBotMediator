using Microsoft.Extensions.DependencyInjection;
using TelegramBotMediator.Application.Abstractions.Services;
using TelegramBotMediator.Application.Services;

namespace TelegramBotMediator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IStateService, StateService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
