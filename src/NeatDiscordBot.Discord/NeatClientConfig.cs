using System.ComponentModel.DataAnnotations;

namespace NeatDiscordBot.Discord;

public class NeatClientConfig
{
    [Required]
    public required string Token { get; set; }
}