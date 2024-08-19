using System.Text.Json.Serialization;
using NeatDiscordBot.Services.Redis.Abstractions;

namespace NeatDiscordBot.Discord.Entities;

public class User : IRedisEntity
{
    private User()
    {
        Nickname = default!;
        Currency = new(0);
    }

    public User(ulong guildId, ulong userId) : this()
    {
        GuildId = guildId;
        UserId = userId;
    }

    public ulong UserId { get; set; }

    public ulong GuildId { get; set; }

    public string? Username { get; set; }

    public string? Nickname { get; set; }

    /// <summary> Represents emoji/emotes collected by user </summary>
    public Dictionary<string, uint> Currency { get; set; }

    [JsonIgnore]
    public TimeSpan TotalVoiceActivity { get; set; }
    
    [JsonPropertyName("TotalVoiceActivity")]
    public string TimeInVoiceSpentString
    {
        get => TotalVoiceActivity.ToString();
        set => TotalVoiceActivity = TimeSpan.TryParse(value, out var result) ? result : default;
    }

    [JsonIgnore]
    public string Mention => $"<@{UserId}>";

    public string GetCacheKey()
        => GetCacheKey(GuildId, UserId);

    public static string GetCacheKey(ulong guildId, ulong userId) 
        => $"{guildId}:users:{userId}";
}