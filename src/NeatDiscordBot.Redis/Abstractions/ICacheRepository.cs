namespace NeatDiscordBot.Redis.Abstractions;

public interface ICacheRepository
{
    ValueTask<T?> GetAsync<T>(string cacheKey) where T : IRedisEntity;

    ValueTask<T[]> GetAsync<T>(IEnumerable<string> cacheKeys) where T : IRedisEntity;

    ValueTask SaveAsync<T>(T entity) where T : IRedisEntity;
}