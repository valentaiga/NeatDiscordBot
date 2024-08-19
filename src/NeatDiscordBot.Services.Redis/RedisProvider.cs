using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.Extensions.Options;
using NeatDiscordBot.Services.Redis.Abstractions;
using StackExchange.Redis;

namespace NeatDiscordBot.Services.Redis;

public class RedisProvider : ICacheProvider, IDisposable
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly string _prefix;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.Cyrillic, UnicodeRanges.BasicLatin)
    };

    public RedisProvider(IOptions<RedisConfiguration> redisConfiguration)
    {
        _prefix = redisConfiguration.Value.Prefix;

        var redisOptions = new ConfigurationOptions()
        {
            EndPoints =
            {
                redisConfiguration.Value.Endpoint
            },
            Password = redisConfiguration.Value.Password
        };
        _multiplexer = ConnectionMultiplexer.Connect(redisOptions);
    }

    public async ValueTask Remove(string cacheKey)
    {
        await Database.KeyDeleteAsync(cacheKey);
    }

    public async ValueTask SaveAsync<T>(string cacheKey, T value, TimeSpan? expirationTime = null)
    {
        var json = SerializeValue(value);
        await Database.StringSetAsync(AdjustProjectPrefix(cacheKey), json, expirationTime);
    }

    public async ValueTask<bool> KeyExistsAsync(string cacheKey)
    {
        return await Database.KeyExistsAsync(cacheKey);
    }

    public async ValueTask<T?> GetFromHashAsync<T>(string outerKey, string innerKey)
    {
        var result = await Database.HashGetAsync(AdjustProjectPrefix(outerKey), innerKey);
        return DeserializeValue<T>(result);
    }

    public async ValueTask<Dictionary<string, TValue>> GetFromHashAllAsync<TValue>(string outerKey)
    {
        var result = await Database.HashGetAllAsync(AdjustProjectPrefix(outerKey));
        return result.ToDictionary(x => x.Name.ToString(), x => DeserializeValue<TValue>(x.Value)!);
    }

    public async ValueTask<bool> HashExistsAsync(string outerKey, string innerKey)
    {
        return await Database.HashExistsAsync(outerKey, innerKey);
    }

    public async ValueTask HashRemoveAsync(string outerKey, string innerKey)
    {
        await Database.HashDeleteAsync(outerKey, innerKey);
    }

    public async ValueTask AddToHashAsync<T>(string outerKey, string innerKey, T value)
    {
        await Database.HashSetAsync(AdjustProjectPrefix(outerKey), innerKey, SerializeValue(value));
    }

    public async ValueTask SetAddAsync<T>(string cacheKey, T value)
    {
        await Database.SetAddAsync(AdjustProjectPrefix(cacheKey), SerializeValue(value));
    }

    public async ValueTask<bool> SetContainsAsync<T>(string cacheKey, T value)
    {
        return await Database.SetContainsAsync(AdjustProjectPrefix(cacheKey), SerializeValue(value));
    }

    public async ValueTask SetAddAsync<T>(string cacheKey, IEnumerable<T> values)
    {
        var redisValues = values.Select(x => new RedisValue(SerializeValue(x))).ToArray();
        await Database.SetAddAsync(AdjustProjectPrefix(cacheKey), redisValues);
    }

    public async ValueTask SetRemoveAsync<T>(string cacheKey, T value)
    {
        var redisValue = new RedisValue(SerializeValue(value));
        await Database.SetRemoveAsync(AdjustProjectPrefix(cacheKey), redisValue);
    }

    public async ValueTask<IEnumerable<T>> SetGetAllAsync<T>(string cacheKey)
    {
        var result = await Database.SetMembersAsync(AdjustProjectPrefix(cacheKey));
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        return result is null ? Enumerable.Empty<T>() : result.Select(x => DeserializeValue<T>(x)!);
    }

    public async ValueTask<IList<T>> PopSetAsync<T>(string cacheKey)
    {
        var result = new List<T>();
        RedisValue value;

        while ((value = await Database.SetPopAsync(AdjustProjectPrefix(cacheKey))).HasValue)
        {
            result.Add(DeserializeValue<T>(value)!);
        }

        return result;
    }

    public async ValueTask<T?> GetAsync<T>(string cacheKey)
    {
        var result = await Database.StringGetAsync(AdjustProjectPrefix(cacheKey));
        return result.HasValue ? DeserializeValue<T>(result) : default;
    }

    public async ValueTask<T[]> GetAsync<T>(IEnumerable<string> cacheKeys)
    {
        var values = await Database.StringGetAsync(cacheKeys.Select(x => (RedisKey)AdjustProjectPrefix(x)).ToArray());
        return values
            .Where(x => x.HasValue)
            .Select(x => DeserializeValue<T>(x))
            .OfType<T>()
            .ToArray();
    }

    private IDatabase Database => _multiplexer.GetDatabase();

    private string AdjustProjectPrefix(string cacheKey) => $"{_prefix}:{cacheKey}";

    private string SerializeValue<T>(T value) => JsonSerializer.Serialize(value, _jsonSerializerOptions);

    private T? DeserializeValue<T>(string? json) =>
        json is not null ? JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions) : default;

    public void Dispose() => _multiplexer.Dispose();
}