namespace Willow.Settings.Events;

/// <summary>
/// Event indicating the settings were updated, this event guarantees a change was made into the file, and that the
/// <i>JSON</i> representation of <see cref="OldValue" /> is not the same as <see cref="NewValue" />.
/// </summary>
/// <param name="OldValue">The old value, which is also nice, but not new.</param>
/// <param name="NewValue">The new, better and improved value of <typeparamref name="T" />.</param>
/// <typeparam name="T">Type of settings.</typeparam>
public readonly record struct SettingsUpdatedEvent<T>(T OldValue, T NewValue);
