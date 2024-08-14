namespace NeatDiscordBot.Discord.Features;

public interface IFeature
{
    string FeatureName { get; }
    void Enable();
}