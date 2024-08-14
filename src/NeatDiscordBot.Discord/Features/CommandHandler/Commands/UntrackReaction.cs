using Discord;
using Discord.WebSocket;
using NeatDiscordBot.Discord.Services;

namespace NeatDiscordBot.Discord.Features.CommandHandler.Commands;

public class UntrackReaction : IBotCommand
{
    public string Name => "untrack_reaction";
    public string Description => "Undo track reaction";
    public GuildPermission RequiredPermission => GuildPermission.Administrator;

    public SlashCommandOptionBuilder[] Options
    {
        get
        {
            var reactionBuilder = new SlashCommandOptionBuilder()
                .WithName("reaction")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithDescription("emote/emoji");
            return new[] { reactionBuilder };
        }
    }

    private readonly IGuildReactionsService _reactionsService;

    public UntrackReaction(IGuildReactionsService reactionsService)
    {
        _reactionsService = reactionsService;
    }

    public async ValueTask Execute(SocketSlashCommand message)
    {
        const string errorMessage = "Command requires 1 emote/emoji to track as parameter";
        
        if (message.Data.Options.Count != 1)
        {
            await message.RespondAsync(errorMessage, ephemeral: true);
            return;
        }

        var value = message.Data.Options.First().Value;
        if (value is not string reactionStr)
        {
            await message.RespondAsync(errorMessage, ephemeral: true);
            return;
        }

        var isRemoved = await _reactionsService.RemoveTrackedReactionAsync(message.GuildId!.Value, reactionStr);
        if (isRemoved)
            await message.RespondAsync($"Removed {reactionStr} from guild tracked reactions");
        else
            await message.RespondAsync($"Reaction {reactionStr} not found in guild tracked reactions");
            
    }
}