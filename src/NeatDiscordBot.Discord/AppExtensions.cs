using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeatDiscordBot.Discord.Features;
using NeatDiscordBot.Discord.Features.CommandHandler;
using NeatDiscordBot.Discord.Features.CommandHandler.Commands;
using NeatDiscordBot.Discord.Features.UserTracker;
using NeatDiscordBot.Discord.Services;
using NeatDiscordBot.Redis;

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
        // infrastructure
        services.AddRedis();
        services.AddOptions<FeatureConfig>()
            .BindConfiguration(FeatureConfig.ConfigurationPath)
            .ValidateDataAnnotations();

        // features
        services
            .AddSingleton<IFeature, UserInfoTracker>()
            .AddSingleton<IFeature, CommandHandler>();

        // commands
        services
            .AddSingleton<IBotCommand, PingCommand>();
        
        // services
        services.AddSingleton<IUserRepository, RedisUserRepository>();

        // client
        services.AddOptions<NeatClientConfig>()
            .BindConfiguration(NeatClientConfig.ConfigurationPath)
            .ValidateDataAnnotations();
        services.AddSingleton<INeatClient, NeatClient>();

        return services;
    }

    public static IServiceProvider EnableFeatures(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IFeature>>();
        var features = serviceProvider.GetServices<IFeature>();
        foreach (var feature in features)
        {
            var isEnabled = serviceProvider.IsFeatureEnabled(feature.FeatureName); 
            logger.Information("Feature {Feature}:{IsEnabled}", feature.FeatureName, isEnabled);
            if (isEnabled)
            {
                feature.Enable();
            }
        }

        return serviceProvider;
    }

    private static bool IsFeatureEnabled(this IServiceProvider serviceProvider, string featureName)
    {
        var options = serviceProvider.GetRequiredService<IOptions<FeatureConfig>>().Value;
        return options.EnableAllFeatures
               || options.EnabledFeatures.Contains(featureName);
    }
}