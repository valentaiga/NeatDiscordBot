using Microsoft.Extensions.Logging;
using NeatDiscordBot.Discord.Entities;
using NeatDiscordBot.Redis.Abstractions;

namespace NeatDiscordBot.Discord.Services;

public interface IUserRepository
{
    ValueTask<User> GetOrCreateUserAsync(ulong userId, ulong guildId);
    ValueTask SaveUserAsync(User user);
}

public class RedisUserRepository : IUserRepository
{
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<RedisUserRepository> _logger;

    public RedisUserRepository(ICacheRepository cacheRepository, ILogger<RedisUserRepository> logger)
    {
        _cacheRepository = cacheRepository;
        _logger = logger;
    }

    public async ValueTask<User> GetOrCreateUserAsync(ulong userId, ulong guildId)
    {
        var user = await _cacheRepository.GetAsync<User>(User.GetCacheKey(guildId, userId));

        if (user is null)
        {
            user = new User(guildId, userId);
            await _cacheRepository.SaveAsync(user);
            _logger.Debug($"User with key '{user.GetCacheKey()}' created");
        }

        return user;
    }

    public async ValueTask SaveUserAsync(User user)
    {
        await _cacheRepository.SaveAsync(user);
        _logger.Debug($"User '{user.UserId}' with key '{user.GetCacheKey()}' updated");
    }
}