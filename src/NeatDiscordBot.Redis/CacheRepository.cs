using NeatDiscordBot.Redis.Abstractions;

namespace NeatDiscordBot.Redis;

public class CacheRepository : ICacheRepository
{
    private readonly ICacheProvider _cacheProvider;

    public CacheRepository(ICacheProvider cacheProvider)
    {
        // if I would fetch all saved guild users check: https://stackexchange.github.io/StackExchange.Redis/KeysScan
        _cacheProvider = cacheProvider;
    }

    public async ValueTask<T?> GetAsync<T>(string cacheKey) where T : IRedisEntity
    {
        return await _cacheProvider.GetAsync<T>(cacheKey);
    }

    public async ValueTask<T[]> GetAsync<T>(IEnumerable<string> cacheKeys) where T : IRedisEntity
    {
        return await _cacheProvider.GetAsync<T>(cacheKeys);
    }

    public async ValueTask SaveAsync<T>(T entity) where T : IRedisEntity
    {
        var cacheKey = entity.GetCacheKey();
        await _cacheProvider.SaveAsync(cacheKey, entity);
    }
}