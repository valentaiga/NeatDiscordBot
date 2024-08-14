using Discord;
using Discord.WebSocket;
using NeatDiscordBot.Discord.Services;

namespace NeatDiscordBot.Discord.Features.UserTracker;

public class ReactionsTracker : IFeature
{
    public string FeatureName => "reactions_tracker";

    private readonly INeatClient _neatClient;
    private readonly IGuildReactionsService _guildReactionsService;

    public ReactionsTracker(INeatClient neatClient, IGuildReactionsService guildReactionsService)
    {
        _neatClient = neatClient;
        _guildReactionsService = guildReactionsService;
    }
    
    public void Enable()
    {
        _neatClient.BotClient.ReactionAdded += BotClientOnReactionAdded;
    }

    private async Task BotClientOnReactionAdded(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedTextChannel,
        SocketReaction reaction)
    {
        var message = await cachedMessage.GetOrDownloadAsync();
        if (message.Author.IsBot) return;

        var textChannel = (SocketGuildChannel)await cachedTextChannel.GetOrDownloadAsync();

        var isReactionTracked =
            await _guildReactionsService.IsReactionTracked(textChannel.Guild.Id, reaction.Emote.Name);
        if (!isReactionTracked) return;
        
        var hasRecentReaction =
            await _guildReactionsService.HasRecentReactionAsync(textChannel.Guild.Id, message.Author.Id, reaction.UserId, reaction.Emote.Name);
        if (hasRecentReaction) return;

        await _guildReactionsService.AddReactionAsync(textChannel.Guild.Id, message.Id, reaction.UserId, reaction.Emote.Name);
    }
}