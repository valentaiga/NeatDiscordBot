using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.WebSocket;
using NeatDiscordBot.Discord.Services;

namespace NeatDiscordBot.Discord.Features.CommandHandler.Commands;

public class TrackReaction : IBotCommand
{
    public string Name => "track_reaction";
    public string Description => "Track reactions to count them on user statistics";
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
    private readonly INeatClient _neatClient;

    public TrackReaction(IGuildReactionsService reactionsService, INeatClient neatClient)
    {
        _reactionsService = reactionsService;
        _neatClient = neatClient;
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

        if (Emoji.TryParse(reactionStr, out _))
        {
            await _reactionsService.AddTrackedReactionAsync(message.GuildId!.Value, reactionStr);
            await message.RespondAsync($"Reaction {reactionStr} is now tracked");
            return;
        }

        if (TryParseReaction(reactionStr, out _, out var reactionId)
            && await IsGuildReactionAsync(message.GuildId!.Value, reactionId.Value))
        {
            await _reactionsService.AddTrackedReactionAsync(message.GuildId!.Value, reactionStr);
            await message.RespondAsync($"Reaction {reactionStr} is now tracked");
            return;
        }

        await message.RespondAsync(errorMessage, ephemeral: true);
    }

    private async ValueTask<bool> IsGuildReactionAsync(ulong guildId, ulong reactionId)
    {
        var guild = _neatClient.BotClient.GetGuild(guildId);
        
        var guildEmote = await guild.GetEmoteAsync(reactionId);
        return guildEmote is not null;
    }
    
    private static bool TryParseReaction(
        object reaction, // <:4HEad:439471081535963146>
        [NotNullWhen(true)] out string? reactionName,
        [NotNullWhen(true)] out ulong? reactionId)
    {
        reactionName = default;
        reactionId = default;
        if (reaction is not string reactionStr) return false;

        var reactionStrSpan = reactionStr.AsSpan();
        short nameStart = -1;
        short idStart = -1;
        short idEnd = -1;

        for (short i = 0; i < reactionStr.Length; i++)
        {
            var c = reactionStrSpan[i]; 
            if (c == ':')
            {
                if (nameStart == -1) nameStart = i;
                else if (idStart == -1) idStart = i;
                else return false;
            }
            else if (idStart != -1 && c == '>' && idEnd == -1)
                idEnd = i;
        }

        if (idStart == -1) return false;

        reactionName = reactionStrSpan[(nameStart + 1)..idStart].ToString();
        if (!ulong.TryParse(reactionStrSpan[(idStart + 1)..idEnd], out var reactionLongId))
            return false;
        reactionId = reactionLongId;
        return true;
    }
}