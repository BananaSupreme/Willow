using Desktop.Robot;

using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.DeviceAutomation.InputDevices;
using Willow.DeviceAutomation.InputDevices.Abstractions;

namespace Willow.DeviceAutomation;

public class DeviceAutomationRegistrator : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IInputSimulator, InputSimulator>();
        services.AddSingleton<IRobot, Robot>();
    }
}