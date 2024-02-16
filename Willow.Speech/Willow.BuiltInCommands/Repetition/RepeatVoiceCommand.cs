using Willow.Eventing;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.BuiltInCommands.Repetition;

internal sealed class RepeatVoiceCommand : IVoiceCommand
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly LastCommandContainer _lastCommandContainer;

    //List because we need to look up the index
    private static readonly List<string> _cardinal
        = "first second third fourth fifth sixth seventh eighth ninth tenth".Split(' ').ToList();

    public RepeatVoiceCommand(IEventDispatcher eventDispatcher, LastCommandContainer lastCommandContainer)
    {
        _eventDispatcher = eventDispatcher;
        _lastCommandContainer = lastCommandContainer;
    }

    public string InvocationPhrase => "[repeat|again]:_";

    public Task ExecuteAsync(VoiceCommandContext context)
    {
        if (_lastCommandContainer.LastCommand == default)
        {
            return Task.CompletedTask;
        }

        _eventDispatcher.Dispatch(_lastCommandContainer.LastCommand);
        return Task.CompletedTask;
    }

    [VoiceCommand("times #number")]
    public Task RepeatTimesVoiceCommand(VoiceCommandContext context)
    {
        if (_lastCommandContainer.LastCommand == default)
        {
            return Task.CompletedTask;
        }

        var repetitions = context.Parameters["number"].GetInt32();
        repetitions = Math.Min(10, repetitions);

        for (var i = 0; i < repetitions; i++)
        {
            _eventDispatcher.Dispatch(_lastCommandContainer.LastCommand);
        }

        return Task.CompletedTask;
    }

    [VoiceCommand("[_cardinal]:cardinal")]
    public Task RepeatCardinalVoiceCommand(VoiceCommandContext context)
    {
        if (_lastCommandContainer.LastCommand == default)
        {
            return Task.CompletedTask;
        }

        var cardinal = context.Parameters["cardinal"].GetString();
        var repetitions = _cardinal.ToList().IndexOf(cardinal) + 1;

        for (var i = 0; i < repetitions; i++)
        {
            _eventDispatcher.Dispatch(_lastCommandContainer.LastCommand);
        }

        return Task.CompletedTask;
    }
}
