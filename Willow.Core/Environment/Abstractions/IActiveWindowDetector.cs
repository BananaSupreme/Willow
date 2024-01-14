using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.Abstractions;

/// <summary>
/// Detects the active foreground window on the user device.
/// </summary>
internal interface IActiveWindowDetector
{
    /// <summary>
    /// Returns the currently active window.
    /// </summary>
    /// <returns>Info about the active window.</returns>
    ActiveWindowInfo GetActiveWindow();
}