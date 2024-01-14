using Willow.Core.Settings.Events;
using Willow.Helpers.Extensions;

namespace Willow.Core.Settings.Abstractions;

/// <summary>
/// A settings object backed by the file system
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>
/// When a file does not exist behind the scenes a new file will be created to store the settings, this file will
/// be based on the default values as defined in the empty constructor.
/// </item>
/// <item>
/// When updates are requested they are queued to be written in the file and expressed immediately in the
/// <see cref="CurrentValue"/> parameter and as an update with <see cref="SettingsUpdatedEvent{T}"/> event.
/// </item>
/// <item>
/// The settings are stored in the <i>JSON</i> format, that is intended to allow the user to fix an issue that for
/// some reason prevents the user from using the system correctly. <br/>
/// If for some reason the file is left in an incorrect state, the system removes it and creates a new one based
/// on the empty constructor.
/// </item>
/// <item>
/// The settings file holds the settings file to prevent it from being modified while the app is running,
/// as this can create many edge cases to consider.
/// </item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of parameters to be loaded.</typeparam>
public interface ISettings<T>
    where T : new()
{
    internal static string SettingsFolderPath => $"./Settings";

    internal static string SettingsFilePath =>
        $"{SettingsFolderPath}/{TypeExtensions.GetFullName<T>().Replace('.', '_')}.json";

    /// <summary>
    /// The most recent value of <typeparamref name="T"/> found in the system.
    /// </summary>
    T CurrentValue { get; }

    /// <summary>
    /// Updates <typeparamref name="T"/> with <see cref="newValue"/>, this update will eventually be synced into the
    /// underlying file.
    /// </summary>
    /// <param name="newValue">The new and updated value of <typeparamref name="T"/>.</param>
    void Update(T newValue);

    /// <summary>
    /// Flushes the syncing into the file, this is primarily for tests and shutdown.
    /// </summary>
    internal void Flush();
}