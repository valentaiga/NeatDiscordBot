namespace NeatDiscordBot.Redis.Abstractions;

public interface IRedisEntity
{
    string GetCacheKey();
}