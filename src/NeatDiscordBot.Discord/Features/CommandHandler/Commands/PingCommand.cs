using Discord.WebSocket;

namespace NeatDiscordBot.Discord.Features.CommandHandler.Commands;

public class PingCommand : IBotCommand
{
    public string Name => "ping";
    public string Description => "Replies with \"pong\"";

    public async ValueTask Execute(SocketSlashCommand message)
    {
        await message.RespondAsync("Pong!", ephemeral: true);
    }
}