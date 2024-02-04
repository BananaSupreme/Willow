using Willow.Environment.Models;

namespace Willow.Environment.Events;

/// <summary>
/// The current active window has changed.
/// </summary>
/// <param name="NewWindow">The new current window.</param>
public readonly record struct ActiveWindowChangedEvent(ActiveWindowInfo NewWindow);
