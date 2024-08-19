using NeatDiscordBot.Discord.Entities;
using NeatDiscordBot.Services.Redis.Abstractions;

namespace NeatDiscordBot.Discord.Services;

public interface IGuildRepository
{
    ValueTask<Guild> GetOrCreateGuildAsync(ulong guildId);
    ValueTask SaveGuildAsync(Guild guild);
}

public class GuildRepository : IGuildRepository
{
    private readonly ICacheRepository _cacheRepository;

    public GuildRepository(ICacheRepository cacheRepository)
    {
        _cacheRepository = cacheRepository;
    }

    public async ValueTask<Guild> GetOrCreateGuildAsync(ulong guildId)
    {
        var guild = await _cacheRepository.GetAsync<Guild>(Guild.GetCacheKey(guildId));
        if (guild is null)
        {
            guild = new Guild(guildId);
            await SaveGuildAsync(guild);
        }

        return guild;
    }

    public ValueTask SaveGuildAsync(Guild guild)
        => _cacheRepository.SaveAsync(guild);
}