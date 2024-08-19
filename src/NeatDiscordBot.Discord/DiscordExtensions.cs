using Discord.WebSocket;

namespace NeatDiscordBot.Discord;

internal static class DiscordExtensions
{
    public static bool IsMuted(this SocketGuildUser user)
        => user.IsMuted || user.IsSelfMuted || user.IsDeafened || user.IsSelfDeafened;
    
    public static bool IsMuted(this SocketVoiceState vc)
        => vc.IsMuted || vc.IsSelfMuted || vc.IsDeafened || vc.IsSelfDeafened;
}