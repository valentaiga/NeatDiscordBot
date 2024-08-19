namespace NeatDiscordBot.Services.Redis.Abstractions;

public interface IRedisEntity
{
    string GetCacheKey();
}