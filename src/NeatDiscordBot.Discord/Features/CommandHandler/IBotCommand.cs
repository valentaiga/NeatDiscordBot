﻿using Discord;
using Discord.WebSocket;

namespace NeatDiscordBot.Discord.Features.CommandHandler;

public interface IBotCommand
{
    string Name { get; }
    string Description { get; }
    GuildPermission RequiredPermission { get; }
    SlashCommandOptionBuilder[] Options { get; }

    ValueTask Execute(SocketSlashCommand message);
}