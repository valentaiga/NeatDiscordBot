using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace NeatDiscordBot.Discord.Features;

public class FeatureConfig
{
    public const string ConfigurationPath = "Features";
    
    [Required]
    public required bool EnableAllFeatures { get; set; }

    public required ImmutableHashSet<string> EnabledFeatures { get; set; }
}