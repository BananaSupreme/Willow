using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Helpers;
using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;

namespace Willow.Core.SpeechCommands.ScriptingInterface;

internal class ScriptingInterfaceRegistrar : IServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IVoiceCommandInterpreter, VoiceCommandInterpreter>();
        services.AddSingleton<IAssemblyRegistrar, AssemblyRegistrar>();
        services.AddSingleton<IEventRegistrar, EventRegistrar>();
        services.AddSingleton<IInterfaceRegistrar, InterfaceRegistrar>();
    }
}