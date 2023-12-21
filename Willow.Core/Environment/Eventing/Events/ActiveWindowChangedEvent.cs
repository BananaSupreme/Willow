using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.Eventing.Events;

public readonly record struct ActiveWindowChangedEvent(ActiveWindowInfo NewWindow);
