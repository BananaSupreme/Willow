using Microsoft.Extensions.Logging;

using Serilog.Context;

using System.Diagnostics.CodeAnalysis;

namespace Willow.Helpers.Logging.Extensions;

public static class LoggerExtensions
{
    /// <summary>
    /// Adds an item to the context of the structured log.
    /// While this item is not disposed every log item added will contain this item
    /// </summary>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <param name="logger">The logger</param>
    /// <param name="name">The name of the property</param>
    /// <param name="property"></param>
    /// <returns></returns>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter",
        Justification = "Its here so it can be used as an extension method")]
    public static IDisposable AddContext(this ILogger logger, string name, object? property)
    {
        return LogContext.PushProperty(name, property);
    }
}