using Microsoft.Extensions.DependencyInjection;
using NeatDiscordBot.Redis.Abstractions;

namespace NeatDiscordBot.Redis;

public static class AppRedisExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services)
    {
        services.AddOptions<RedisConfiguration>()
            .BindConfiguration(RedisConfiguration.ConfigurationPath)
            .ValidateDataAnnotations();

        services.AddSingleton<ICacheProvider, RedisProvider>();
        services.AddSingleton<ICacheRepository, CacheRepository>();

        return services;
    }
}