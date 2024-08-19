using System.Collections.Frozen;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeatDiscordBot.Discord.Services;

namespace NeatDiscordBot.Discord.Features.CommandHandler;

public class CommandHandler : IFeature
{
    public string FeatureName => "command_handler";

    private readonly INeatClient _neatClient;
    private readonly IGuildRepository _guildRepository;
    private readonly NeatClientConfig _options;
    private readonly ILogger<CommandHandler> _logger;

    private readonly FrozenDictionary<string, IBotCommand> _commands;

    public CommandHandler(
        INeatClient neatClient,
        IGuildRepository guildRepository,
        IOptions<NeatClientConfig> options,
        IEnumerable<IBotCommand> commands,
        ILogger<CommandHandler> logger)
    {
        _neatClient = neatClient;
        _guildRepository = guildRepository;
        _options = options.Value;
        _logger = logger;
        _commands = commands.ToFrozenDictionary(x => x.Name);
    }

    public void Enable()
    {
        _neatClient.BotClient.Ready += UpdateSlashCommandsAsync;
        _neatClient.BotClient.JoinedGuild += CreateSlashCommandsAsync;
        _neatClient.BotClient.SlashCommandExecuted += HandleMessageAsync;
    }

    private async Task UpdateSlashCommandsAsync()
    {
        var guilds = _neatClient.BotClient.Guilds;
        foreach (var guild in guilds)
        {
            var guildInfo = await _guildRepository.GetOrCreateGuildAsync(guild.Id);
            if (guildInfo.CommandsVersion == _options.CommandsVersion)
                continue;

            await DeleteSlashCommandsAsync(guild);
            await CreateSlashCommandsAsync(guild);
        }
    }

    private async Task CreateSlashCommandsAsync(SocketGuild guild)
    {
        var guildInfo = await _guildRepository.GetOrCreateGuildAsync(guild.Id);
        var slashCommands = GetSlashCommandsProperties();
        var created = await CreateSlashCommandsAsync(guild, slashCommands);
        if (created)
        {
            guildInfo.CommandsVersion = _options.CommandsVersion;
            await _guildRepository.SaveGuildAsync(guildInfo);
        }
    }

    private async Task HandleMessageAsync(SocketSlashCommand message)
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

    private async ValueTask DeleteSlashCommandsAsync(SocketGuild guild)
    {
        try
        {
            await guild.DeleteApplicationCommandsAsync();
            _logger.Debug("Commands deleted on guild {GuildId} ({GuildName})",
                guild.Id, guild.Name);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Cannot delete commands on guild {GuildId} ({GuildName}}",
                guild.Id, guild.Name);
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

    private async ValueTask<bool> CreateSlashCommandsAsync(SocketGuild guild, SlashCommandProperties[] slashCommands)
    {
        var result = true;
        foreach (var slashCommand in slashCommands)
        {
            try
            {
                await guild.CreateApplicationCommandAsync(slashCommand);
                _logger.Debug("Command {CommandName} created on guild {GuildId} ({GuildName})",
                    slashCommand.Name.Value, guild.Id, guild.Name);
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "Cannot create slash command {CommandName} on guild {GuildId} ({GuildName})",
                    slashCommand.Name, guild.Id, guild.Name);
            }
        }

        return result;
    }
}