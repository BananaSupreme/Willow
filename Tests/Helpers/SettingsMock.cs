using Willow.Core.Settings.Abstractions;

namespace Tests.Helpers;

internal class SettingsMock<T> : ISettings<T>
    where T : new()
{
    public T CurrentValue => new();
    public void Update(T newValue)
    {
        throw new InvalidOperationException();
    }

    public void Flush()
    {
        throw new InvalidOperationException();
    }
}