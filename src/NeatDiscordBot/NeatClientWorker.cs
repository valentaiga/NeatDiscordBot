using NeatDiscordBot.Common;
using NeatDiscordBot.Discord;

namespace NeatDiscordBot;

public class NeatClientWorker : IHostedService
{
    private readonly INeatClient _neatClient;
    private readonly ILogger<NeatClientWorker> _logger;

    public NeatClientWorker(INeatClient neatClient, ILogger<NeatClientWorker> logger)
    {
        _neatClient = neatClient;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.Information("Bot starting at: {time}", DateTimeOffset.UtcNow);
            await _neatClient.StartAsync();
            _logger.Information("Bot started");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Bot cannot start");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _neatClient.StopAsync();
    }
}