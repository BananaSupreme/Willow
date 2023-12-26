using Serilog.Context;

using System.Diagnostics.CodeAnalysis;

namespace Willow.Core.Logging.Extensions;

internal static class LoggerExtensions
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter",
        Justification = "Its here so we can use it as an extension method")]
    public static IDisposable AddContext(this ILogger logger, string name, object? property)
    {
        return LogContext.PushProperty(name, property);
    }
}