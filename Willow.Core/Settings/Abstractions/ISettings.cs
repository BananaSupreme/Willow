using Willow.Helpers.Extensions;

namespace Willow.Core.Settings.Abstractions;

public interface ISettings<T>
    where T : new()
{
    internal static string SettingsFolderPath => $"./Settings";

    internal static string SettingsFilePath =>
        $"{SettingsFolderPath}/{TypeExtensions.GetFullName<T>().Replace('.', '_')}.json";

    T CurrentValue { get; }
    void Update(T newValue);
    internal void Flush();
}