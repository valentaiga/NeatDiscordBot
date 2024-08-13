using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace NeatDiscordBot.Discord;

public static class AppExtensions
{
    public static IServiceCollection AddDiscordApi(this IServiceCollection services, Action<DiscordSocketConfig>? configureConfig = null)
    {
        services.AddOptions<DiscordSocketConfig>()
            .Configure(c => configureConfig?.Invoke(c))
            .ValidateDataAnnotations();

        services.AddSingleton<IDiscordClient>(sp => sp.GetRequiredService<INeatClient>().BotClient);
        return services;
    }

    public static IServiceCollection AddNeatServices(this IServiceCollection services)
    {
        services.AddOptions<NeatClientConfig>()
            .Configure(config =>
            {
                config.Token = Environment.GetEnvironmentVariable("NEAT_TOKEN")!;
            })
            .ValidateDataAnnotations();
        services.AddSingleton<INeatClient, NeatClient>();
        return services;
    }
}