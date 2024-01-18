using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.ScriptingInterface.Registration;

internal sealed class ScriptingInterfaceRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAssemblyRegistrar, ScriptingInterfaceAssemblyRegistrar>();
        services.AddSingleton<IVoiceCommandInterpreter, VoiceCommandInterpreter>();
    }
}
