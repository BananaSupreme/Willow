namespace Willow.Core.Environment.Models;

/// <summary>
/// Information about the active window
/// </summary>
/// <param name="ProcessName">The process name associated with window in the host computer</param>
public readonly record struct ActiveWindowInfo(string ProcessName);
