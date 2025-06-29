using System.Runtime.CompilerServices;

namespace SimpleVote.Server.Utils;

public static class LogHelper
{
    /// <summary>Print an information level log along with the log caller name</summary>
    /// <param name="formattedLogText">Pure formatted log text without containing 
    /// any reference to the caller</param>
    /// <param name="callerName">Do not pass anything to this. Leave it empty.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <inheritdoc cref="LoggerExtensions.LogInformation(ILogger, EventId, Exception?, string?, object?[])"/>
    public static void LogInformation(ILogger logger, string formattedLogText,
        Exception? exception = null, EventId eventId = default,
        [CallerMemberName] string callerName = "")
    {
        ArgumentNullException.ThrowIfNull(logger);
        logger.LogInformation(eventId, exception, "({caller}): {logText}", callerName, formattedLogText);
    }

    /// <summary>Print a warning level log along with the log caller name</summary>
    /// <inheritdoc cref="LogInformation"/>
    public static void LogWarning(ILogger logger, string logText,
        Exception? exception = null, EventId eventId = default,
        [CallerMemberName] string callerName = "")
    {
        ArgumentNullException.ThrowIfNull(logger);
        logger.LogWarning(eventId, exception, "({caller}): {logText}", callerName, logText);
    }

    /// <summary>Print an error level log along with the log caller name</summary>
    /// <inheritdoc cref="LogInformation"/>
    public static void LogError(ILogger logger, string logText,
        Exception? exception = null, EventId eventId = default,
        [CallerMemberName] string callerName = "")
    {
        ArgumentNullException.ThrowIfNull(logger);
        logger.LogError(eventId, exception, "({caller}): {logText}", callerName, logText);
    }

    /// <summary>Print a critical level log along with the log caller name</summary>
    /// <inheritdoc cref="LogInformation"/>
    public static void LogCritical(ILogger logger, string logText,
        Exception? exception = null, EventId eventId = default,
        [CallerMemberName] string callerName = "")
    {
        ArgumentNullException.ThrowIfNull(logger);
        logger.LogCritical(eventId, exception, "({caller}): {logText}", callerName, logText);
    }
    
    /// <summary>Print a debug level log along with the log caller name</summary>
    /// <inheritdoc cref="LogInformation"/>
    public static void LogDebug(ILogger logger, string logText,
        Exception? exception = null, EventId eventId = default,
        [CallerMemberName] string callerName = "")
    {
        ArgumentNullException.ThrowIfNull(logger);
        logger.LogDebug(eventId, exception, "({caller}): {logText}", callerName, logText);
    }
}