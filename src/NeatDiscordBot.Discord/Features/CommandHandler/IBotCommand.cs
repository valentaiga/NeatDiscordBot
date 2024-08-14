using Discord.WebSocket;

namespace NeatDiscordBot.Discord.Features.CommandHandler;

public interface IBotCommand
{
    string Name { get; }
    string Description { get; }
    ValueTask Execute(SocketSlashCommand message);
}