using Discord;
using Discord.WebSocket;

namespace NeatDiscordBot.Discord.Features.CommandHandler.Commands;

public class PingCommand : IBotCommand
{
    public string Name => "ping";
    public string Description => "Replies with \"pong\"";
    public GuildPermission RequiredPermission => GuildPermission.SendMessages;
    public SlashCommandOptionBuilder[] Options => Array.Empty<SlashCommandOptionBuilder>();

    public async ValueTask Execute(SocketSlashCommand message)
    {
        await message.RespondAsync("Pong!", ephemeral: true);
    }
}