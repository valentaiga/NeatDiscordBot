using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace NeatDiscordBot.Discord;

public interface INeatClient
{
    DiscordSocketClient BotClient { get; }
    ValueTask StartAsync();
    ValueTask StopAsync();
}

public class NeatClient : INeatClient
{
    public DiscordSocketClient BotClient => _botClient;
    
    private readonly DiscordSocketClient _botClient;
    private readonly NeatClientConfig _config;

    public NeatClient(IOptions<DiscordSocketConfig> botConfig, IOptions<NeatClientConfig> clientConfig)
    {
        _botClient = new DiscordSocketClient(botConfig.Value);
        _config = clientConfig.Value;
    }

    public async ValueTask StartAsync()
    {
        await _botClient.LoginAsync(TokenType.Bot, _config.Token);
        await _botClient.StartAsync();
    }

    public async ValueTask StopAsync()
    {
        await _botClient.StopAsync();
    }
}