using Microsoft.Extensions.Logging;

namespace NeatDiscordBot.Common;

public static class LoggerExtensions
{
    public static void Debug(this ILogger logger, string message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug(message, args);
    }

    public static void Information(this ILogger logger, string message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(message, args);
    }

    public static void Warning(this ILogger logger, string message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning(message, args);
    }

    public static void Error(this ILogger logger, Exception ex, string message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Error))
            logger.LogError(ex, message, args);
    }
}