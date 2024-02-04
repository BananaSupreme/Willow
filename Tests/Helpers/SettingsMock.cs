using Willow.Settings;

namespace Tests.Helpers;

internal sealed class SettingsMock<T> : ISettings<T> where T : new()
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
