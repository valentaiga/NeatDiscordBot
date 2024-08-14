using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace NeatDiscordBot.Discord;

public static class DiscordExtensions
{
    public static bool IsSocketGuildUser(this SocketUser socketUser, [NotNullWhen(true)] out SocketGuildUser? dsUser)
    {
        dsUser = socketUser as SocketGuildUser;
        return dsUser is not null;
    }
}