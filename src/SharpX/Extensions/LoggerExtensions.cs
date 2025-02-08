using Microsoft.Extensions.Logging;

namespace SharpX.Extensions;

public static class LoggerExtensions
{
    public static T? FailWith<T>(
        this ILogger logger, string message, T? returnValue = default, params object[] args)
    {
        Guard.DisallowNull(nameof(logger), logger);
        Guard.DisallowNull(nameof(message), message);

        logger.LogError(message, args);

        return returnValue;
    }

    public static T? PanicWith<T>(
        this ILogger logger, string message, Exception? ex = null, T? returnValue = default, params object[] args)
    {
        Guard.DisallowNull(nameof(logger), logger);
        Guard.DisallowNull(nameof(message), message);

        logger.LogCritical(ex, message, args);

        return returnValue;
    }
}
