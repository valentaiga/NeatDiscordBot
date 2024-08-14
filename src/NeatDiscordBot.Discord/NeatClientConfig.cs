using System.ComponentModel.DataAnnotations;

namespace NeatDiscordBot.Discord;

public class NeatClientConfig
{
    public const string ConfigurationPath = "Neat";
    
    [Required]
    public required string Token { get; set; }
}