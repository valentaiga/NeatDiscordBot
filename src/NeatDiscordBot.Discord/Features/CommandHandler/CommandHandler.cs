using System.Collections.Frozen;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace NeatDiscordBot.Discord.Features.CommandHandler;

public class CommandHandler : IFeature
{
    public string FeatureName => "CommandHandler";

    private readonly INeatClient _neatClient;
    private readonly ILogger<CommandHandler> _logger;
    private readonly FrozenDictionary<string, IBotCommand> _commands;

    public CommandHandler(INeatClient neatClient, IEnumerable<IBotCommand> commands, ILogger<CommandHandler> logger)
    {
        _neatClient = neatClient;
        _logger = logger;
        _commands = commands.ToFrozenDictionary(x => x.Name);
    }
    
    public void Enable()
    {
        _neatClient.BotClient.Ready += CreateSlashCommands;
        _neatClient.BotClient.Ready += DeleteDeprecatedSlashCommands;
        _neatClient.BotClient.SlashCommandExecuted += HandleMessage;
    }

    private async Task HandleMessage(SocketSlashCommand message)
    {
        if (message.User.IsBot) return;
        if (!_commands.TryGetValue(message.CommandName, out var command))
        {
            await message.RespondAsync("Command not found");
            return;
        }

        try
        {
            await command.Execute(message);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Command {CommandName} threw an error", command.Name);
        }
    }

    private async Task DeleteDeprecatedSlashCommands()
    {
        var guilds = _neatClient.BotClient.Guilds;
        foreach (var guild in guilds)
        {
            var guildCommands = await guild.GetApplicationCommandsAsync();
            foreach (var slashCommand in guildCommands)
            {
                if (_commands.ContainsKey(slashCommand.Name)) continue;

                try
                {
                    await guild.DeleteIntegrationAsync(slashCommand.Id);
                    _logger.Debug("Command {CommandName} deleted on guild {GuildName} ({GuildId}}",
                        slashCommand.Name, guild.Id, guild.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Cannot delete command {CommandName} on guild {GuildName} ({GuildId}}",
                        slashCommand.Name, guild.Id, guild.Name);
                }
            }
        }
    }

    private async Task CreateSlashCommands()
    {
        var slashCommands = new SlashCommandProperties[_commands.Count];
        var idx = 0;
        foreach (var (_, command) in _commands)
        {
            var slashCommand = new SlashCommandBuilder()
                .WithName(command.Name)
                .WithDescription(command.Description)
                .Build();
            slashCommands[idx++] = slashCommand;
        }

        var guilds = _neatClient.BotClient.Guilds;
        foreach (var guild in guilds)
        {
            var guildCommands = await guild.GetApplicationCommandsAsync();
            foreach (var slashCommand in slashCommands)
            {
                if (guildCommands.Any(x => x.Name == slashCommand.Name.Value)) continue;
                try
                {
                    await guild.CreateApplicationCommandAsync(slashCommand);
                    _logger.Debug("Command {CommandName} created on guild {GuildName} ({GuildId})",
                        slashCommand.Name.Value, guild.Id, guild.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Cannot create slash command {CommandName} on guild {GuildName} ({GuildId})",
                        slashCommand.Name, guild.Id, guild.Name);
                }
            }
        }
    }
}