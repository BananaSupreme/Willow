using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.Eventing.Events;

/// <summary>
/// The current active window has changed.
/// </summary>
/// <param name="NewWindow">The new current window.</param>
public readonly record struct ActiveWindowChangedEvent(ActiveWindowInfo NewWindow);
