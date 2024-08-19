using NeatDiscordBot.Services.Redis.Abstractions;

namespace NeatDiscordBot.Discord.Services;

public interface IGuildReactionsService
{
    ValueTask<bool> HasRecentReactionAsync(ulong guildId, ulong messageAuthorId, ulong reactionAuthorId, string reactionName);
    ValueTask AddReactionAsync(ulong guildId, ulong messageAuthorId, ulong reactionAuthorId, string reactionName);
    ValueTask AddTrackedReactionAsync(ulong guildId, string reactionName);
    ValueTask<bool> RemoveTrackedReactionAsync(ulong guildId, string reactionName);
    ValueTask<bool> IsReactionTracked(ulong guildId, string reactionName);
}

public class GuildReactionsService : IGuildReactionsService
{
    private readonly IGuildRepository _guildRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICacheProvider _cacheProvider;

    public GuildReactionsService(
        IGuildRepository guildRepository,
        IUserRepository userRepository,
        ICacheProvider cacheProvider)
    {
        _guildRepository = guildRepository;
        _userRepository = userRepository;
        _cacheProvider = cacheProvider;
    }

    public async ValueTask<bool> HasRecentReactionAsync(ulong guildId, ulong messageAuthorId, ulong reactionAuthorId, string reactionName)
    {
        var key = GetRecentReactionCacheKey(guildId, messageAuthorId, reactionAuthorId, reactionName);
        return await _cacheProvider.KeyExistsAsync(key);
    }

    public async ValueTask AddReactionAsync(ulong guildId, ulong messageAuthorId, ulong reactionAuthorId, string reactionName)
    {
        var user = await _userRepository.GetOrCreateUserAsync(guildId, messageAuthorId);
        var reactionsCount = user.Currency.GetValueOrDefault(reactionName);
        user.Currency[reactionName] = ++reactionsCount;
        await _userRepository.SaveUserAsync(user);
        await AddRecentReactionAsync(guildId, messageAuthorId, reactionAuthorId, reactionName);
    }
    
    private async ValueTask AddRecentReactionAsync(ulong guildId, ulong messageAuthorId, ulong reactionAuthorId, string reactionName)
    {
        var key = GetRecentReactionCacheKey(guildId, messageAuthorId, reactionAuthorId, reactionName);
        await _cacheProvider.SaveAsync(key, reactionName, TimeSpan.FromHours(1));
    }

    public async ValueTask<bool> IsReactionTracked(ulong guildId, string reactionName)
    {
        var guild = await _guildRepository.GetOrCreateGuildAsync(guildId);
        return guild.TrackedReactions.Contains(reactionName);
    }

    public async ValueTask<bool> RemoveTrackedReactionAsync(ulong guildId, string reactionName)
    {
        var guild = await _guildRepository.GetOrCreateGuildAsync(guildId);
        if (!guild.TrackedReactions.Contains(reactionName))
            return false;
        guild.TrackedReactions.Remove(reactionName);
        await _guildRepository.SaveGuildAsync(guild);
        return true;
    }

    public async ValueTask AddTrackedReactionAsync(ulong guildId, string reactionName)
    {
        var guild = await _guildRepository.GetOrCreateGuildAsync(guildId);
        if (guild.TrackedReactions.Contains(reactionName))
            return;
        guild.TrackedReactions.Add(reactionName);
        await _guildRepository.SaveGuildAsync(guild);
    }

    private static string GetRecentReactionCacheKey(ulong guildId, ulong messageAuthorId, ulong reactionAuthorId, string reactionName) =>
        $"{guildId}:recentReactions:{reactionName}:{messageAuthorId}:{reactionAuthorId}";
}