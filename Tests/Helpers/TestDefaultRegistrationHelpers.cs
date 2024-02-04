using Microsoft.Extensions.Logging;

using Willow.Settings;

using Xunit.Abstractions;

namespace Tests.Helpers;

public static class TestDefaultRegistrationHelpers
{
    public static void AddSettings(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ISettings<>), typeof(SettingsMock<>));
    }

    public static void AddTestLogger(this IServiceCollection services, ITestOutputHelper testOutputHelper)
    {
        services.AddSingleton(testOutputHelper);
        services.AddSingleton<ILoggerFactory, TestLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
    }
}