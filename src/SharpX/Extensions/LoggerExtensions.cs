using Microsoft.Extensions.Logging;

namespace SharpX.Extensions;

public static class LoggerExtensions
{
    public static T? FailWith<T>(this ILogger logger, string message, T? returnValue = default)
    {
        Guard.DisallowNull(nameof(logger), logger);

        logger.LogError(message);
        return returnValue;
    }

    public static T? PanicWith<T>(this ILogger logger, string message, Exception? ex = null, T? returnValue = default)
    {
        Guard.DisallowNull(nameof(logger), logger);

        logger.LogCritical(ex, message);
        return returnValue;
    }
}
