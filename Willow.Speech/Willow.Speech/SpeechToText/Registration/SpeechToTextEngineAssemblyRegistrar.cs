using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Helpers.Extensions;
using Willow.Registration;
using Willow.Settings;
using Willow.Speech.SpeechToText.Exceptions;
using Willow.Speech.SpeechToText.Settings;

namespace Willow.Speech.SpeechToText.Registration;

internal sealed class SpeechToTextEngineAssemblyRegistrar : IAssemblyRegistrar
{
    private readonly ISettings<SelectedSpeechEngineSettings> _setting;
    private readonly IServiceProvider _systemServiceProvider;

    public SpeechToTextEngineAssemblyRegistrar(ISettings<SelectedSpeechEngineSettings> setting,
                                               IServiceProvider systemServiceProvider)
    {
        _setting = setting;
        _systemServiceProvider = systemServiceProvider;
    }

    public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
    {
        services.AddAllTypesDeriving<ISpeechToTextEngine>(assembly);
    }

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        var engines = _systemServiceProvider.GetServices<ISpeechToTextEngine>().ToArray();
        var duplicateEngineName = engines.Select(static x => x.Name)
                                         .GroupBy(static x => x)
                                         .FirstOrDefault(static x => x.Count() > 1);

        if (duplicateEngineName is not null)
        {
            throw new EnginesMustHaveUniqueNameException(duplicateEngineName.Key);
        }

        if (_setting.CurrentValue.SelectedSpeechEngine is not null)
        {
            return Task.CompletedTask;
        }

        var engineName = engines.Select(static x => x.Name).FirstOrDefault();
        _setting.Update(new SelectedSpeechEngineSettings(engineName));

        return Task.CompletedTask;
    }

    public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        var assemblyEngines = serviceProvider.GetServices<ISpeechToTextEngine>().ToArray();
        var runningEngineIsRemovedWithPlugin = assemblyEngines.Select(static x => x.Name)
                                                              .Contains(_setting.CurrentValue.SelectedSpeechEngine);
        if (!runningEngineIsRemovedWithPlugin)
        {
            return Task.CompletedTask;
        }

        var allEngines = _systemServiceProvider.GetServices<ISpeechToTextEngine>();
        var engineToRun = allEngines.Select(static x => x.Name)
                                    .Except(assemblyEngines.Select(static x => x.Name))
                                    .FirstOrDefault();
        _setting.Update(new SelectedSpeechEngineSettings(engineToRun));
        return Task.CompletedTask;
    }
}
