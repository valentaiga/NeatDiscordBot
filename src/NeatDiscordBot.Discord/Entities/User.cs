using System.Text.Json.Serialization;
using NeatDiscordBot.Redis.Abstractions;

namespace NeatDiscordBot.Discord.Entities;

public class User : IRedisEntity
{
    private User()
    {
        Nickname = default!;
        CollectedReactions = new(0);
    }

    public User(ulong guildId, ulong userId) : this()
    {
        GuildId = guildId;
        UserId = userId;
    }

    public ulong UserId { get; set; }
        
    public ulong GuildId { get; set; }
    public string Nickname { get; set; }
    public Dictionary<string, uint> CollectedReactions { get; set; }

    [JsonIgnore]
    public string Mention => $"<@{UserId}>";

    public string GetCacheKey()
        => GetCacheKey(GuildId, UserId);

    public static string GetCacheKey(ulong guildId, ulong userId) 
        => $"{guildId}:users:{userId}";
}