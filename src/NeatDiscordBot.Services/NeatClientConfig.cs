﻿using System.ComponentModel.DataAnnotations;

namespace NeatDiscordBot.Discord;

public class NeatClientConfig
{
    public const string ConfigurationPath = "Neat";

    [Required]
    public required string Token { get; set; }

    [Required]
    public required TimeSpan ReactionCollectTimeout { get; set; }

    [Required]
    public TimeSpan MinimalVoiceActivity { get; set; }

    [Required]
    public  required string CommandsVersion { get; set; }
}