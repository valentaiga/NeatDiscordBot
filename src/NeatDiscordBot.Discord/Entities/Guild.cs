using System.Text.Json.Serialization;
using NeatDiscordBot.Redis.Abstractions;

namespace NeatDiscordBot.Discord.Entities;

public class Guild : IRedisEntity
{
    [JsonConstructor]
    public Guild()
    {
        IgnoredChannels = new HashSet<ulong>(0);
        TrackedReactions = new HashSet<string>(0);
    }

    public Guild(ulong guildId) : this()
    {
        GuildId = guildId;
    }

    public ulong GuildId { get; set; }

    public HashSet<ulong> IgnoredChannels { get; set; }
    public HashSet<string> TrackedReactions { get; set; }

    public string GetCacheKey()
        => GetCacheKey(GuildId);

    public static string GetCacheKey(ulong guildId)
        => $"{guildId}:guildSettings";
}