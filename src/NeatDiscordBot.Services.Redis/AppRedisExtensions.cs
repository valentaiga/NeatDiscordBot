using Microsoft.Extensions.DependencyInjection;
using NeatDiscordBot.Services.Redis.Abstractions;

namespace NeatDiscordBot.Services.Redis;

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