namespace Willow.Core.Settings.Events;

public readonly record struct SettingsUpdatedEvent<T>(T OldValue, T NewValue);