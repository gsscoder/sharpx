using Microsoft.Extensions.Logging;

namespace SharpX.Extensions;

public static class LoggerExtensions
{
    public static T? WarnWith<T>(
        this ILogger logger, string message, T returnValue, params object[] args)
    {
        Guard.DisallowNull(nameof(logger), logger);
        Guard.DisallowNull(nameof(message), message);
        if (!typeof(T).IsValueType) Guard.DisallowNull(nameof(returnValue), returnValue);

        logger.LogWarning(message, args);

        return returnValue;
    }

    public static T? FailWith<T>(
        this ILogger logger, string message, T returnValue, params object[] args)
    {
        Guard.DisallowNull(nameof(logger), logger);
        Guard.DisallowNull(nameof(message), message);
        if (!typeof(T).IsValueType) Guard.DisallowNull(nameof(returnValue), returnValue);

        logger.LogError(message, args);

        return returnValue;
    }

    public static T? PanicWith<T>(
        this ILogger logger, string message, T returnValue, Exception? ex = null, params object[] args)
    {
        Guard.DisallowNull(nameof(logger), logger);
        Guard.DisallowNull(nameof(message), message);
        if (!typeof(T).IsValueType) Guard.DisallowNull(nameof(returnValue), returnValue);

        logger.LogCritical(ex, message, args);

        return returnValue;
    }
}
