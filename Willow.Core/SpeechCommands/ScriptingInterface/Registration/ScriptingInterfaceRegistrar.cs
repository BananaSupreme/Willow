using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Registration.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

namespace Willow.Core.SpeechCommands.ScriptingInterface.Registration;

internal sealed class ScriptingInterfaceRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAssemblyRegistrar, ScriptingInterfaceAssemblyRegistrar>();
        services.AddSingleton<IVoiceCommandInterpreter, VoiceCommandInterpreter>();
    }
}