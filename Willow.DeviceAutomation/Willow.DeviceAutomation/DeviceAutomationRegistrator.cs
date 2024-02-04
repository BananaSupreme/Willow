using Desktop.Robot;

using Microsoft.Extensions.DependencyInjection;

using Willow.DeviceAutomation.InputDevices;
using Willow.Registration;

namespace Willow.DeviceAutomation;

public sealed class DeviceAutomationRegistrator : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IInputSimulator, InputSimulator>();
        services.AddSingleton<IRobot, Robot>();
    }
}
