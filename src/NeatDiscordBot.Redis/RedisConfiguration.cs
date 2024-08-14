using System.ComponentModel.DataAnnotations;

namespace NeatDiscordBot.Redis;

public class RedisConfiguration
{
    public const string ConfigurationPath = "Redis";
    
    [Required]
    public required string Endpoint { get; set; }

    public required string Password { get; set; }

    [Required]
    public required string Prefix { get; set; }
}