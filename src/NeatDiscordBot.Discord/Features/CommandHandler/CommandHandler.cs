using System.Collections.Frozen;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace NeatDiscordBot.Discord.Features.CommandHandler;

public class CommandHandler : IFeature
{
    public string FeatureName => "command_handler";

    private readonly INeatClient _neatClient;
    private readonly ILogger<CommandHandler> _logger;
    private readonly FrozenDictionary<string, IBotCommand> _commands;

    public CommandHandler(
        INeatClient neatClient,
        IEnumerable<IBotCommand> commands,
        ILogger<CommandHandler> logger)
    {
        _neatClient = neatClient;
        _logger = logger;
        _commands = commands.ToFrozenDictionary(x => x.Name);
    }

    public void Enable()
    {
        _neatClient.BotClient.Ready += DeleteSlashCommandsOnAllGuilds;
        _neatClient.BotClient.Ready += CreateSlashCommandsOnAllGuilds;
        _neatClient.BotClient.JoinedGuild += CreateSlashCommandsOnJoin;
        _neatClient.BotClient.SlashCommandExecuted += HandleMessage;
    }

    private async Task CreateSlashCommandsOnJoin(SocketGuild guild)
    {
        var slashCommands = GetSlashCommandsProperties();
        await CreateCommandsAsync(guild, slashCommands);
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

    private async Task DeleteSlashCommandsOnAllGuilds()
    {
        var guilds = _neatClient.BotClient.Guilds;
        foreach (var guild in guilds)
        {
            try
            {
                await guild.DeleteApplicationCommandsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Cannot delete commands on guild {GuildName} ({GuildId}}",
                    guild.Id, guild.Name);
            }
        }
    }

    private SlashCommandProperties[] GetSlashCommandsProperties()
    {
        var slashCommands = new SlashCommandProperties[_commands.Count];
        var idx = 0;
        foreach (var (_, command) in _commands)
        {
            var slashCommand = new SlashCommandBuilder()
                .WithName(command.Name)
                .WithDescription(command.Description)
                .WithDefaultMemberPermissions(command.RequiredPermission)
                .WithContextTypes(InteractionContextType.Guild)
                .AddOptions(command.Options)
                .Build();
            slashCommands[idx++] = slashCommand;
        }

        return slashCommands;
    }

    private async ValueTask CreateCommandsAsync(SocketGuild guild, SlashCommandProperties[] slashCommands)
    {
        foreach (var slashCommand in slashCommands)
        {
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

    private async Task CreateSlashCommandsOnAllGuilds()
    {
        var slashCommands = GetSlashCommandsProperties();

        foreach (var guild in _neatClient.BotClient.Guilds)
            await CreateCommandsAsync(guild, slashCommands);

        _logger.Information("Commands were created on all connected guilds.");
    }
}