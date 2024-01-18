using Willow.Core.Settings.Abstractions;

namespace Tests.Helpers;

public static class TestDefaultRegistrationHelpers
{
    public static void AddSettings(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ISettings<>), typeof(SettingsMock<>));
    }
}
