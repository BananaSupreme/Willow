using Microsoft.Extensions.DependencyInjection;

using Willow.Registration;
using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.ScriptingInterface.Registration;

internal sealed class ScriptingInterfaceRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IVoiceCommandInterpreter, VoiceCommandInterpreter>();
    }
}
