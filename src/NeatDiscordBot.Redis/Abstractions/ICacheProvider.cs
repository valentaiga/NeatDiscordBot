namespace NeatDiscordBot.Redis.Abstractions;

public interface ICacheProvider
{
    ValueTask Remove(string cacheKey);

    ValueTask SaveAsync<T>(string cacheKey, T value);

    ValueTask<T?> GetAsync<T>(string cacheKey);

    ValueTask<T[]> GetAsync<T>(IEnumerable<string> cacheKeys);

    ValueTask AddToHashAsync<T>(string outerKey, string innerKey, T value);

    ValueTask<bool> HashExistsAsync(string outerKey, string innerKey);

    ValueTask HashRemoveAsync(string outerKey, string innerKey);

    ValueTask<Dictionary<string, TValue>> GetFromHashAllAsync<TValue>(string outerKey);

    ValueTask<T?> GetFromHashAsync<T>(string outerKey, string innerKey);

    ValueTask SetAddAsync<T>(string cacheKey, T value);

    ValueTask SetRemoveAsync<T>(string cacheKey, T value);

    ValueTask<IEnumerable<T>> SetGetAllAsync<T>(string cacheKey);

    ValueTask<bool> SetContainsAsync<T>(string cacheKey, T value);

    ValueTask SetAddAsync<T>(string cacheKey, IEnumerable<T> values);

    ValueTask<IList<T>> PopSetAsync<T>(string cacheKey);
}