using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;
using Willow.Core.Helpers.Logging;
using Willow.Core.SpeechCommands.ScriptingInterface.Abstractions;
using Willow.Core.SpeechCommands.ScriptingInterface.Attributes;
using Willow.Core.SpeechCommands.ScriptingInterface.Models;

namespace Willow.Core.SpeechCommands.ScriptingInterface;

internal class VoiceCommandInterpreter : IVoiceCommandInterpreter
{
    private readonly ILogger<VoiceCommandInterpreter> _log;
    private readonly IServiceProvider _serviceProvider;

    public VoiceCommandInterpreter(ILogger<VoiceCommandInterpreter> log, IServiceProvider serviceProvider)
    {
        _log = log;
        _serviceProvider = serviceProvider;
    }

    public RawVoiceCommand InterpretCommand(IVoiceCommand voiceCommand)
    {
        var voiceCommandType = voiceCommand.GetType();
        var capturedValues = CaptureValues(voiceCommand);
        capturedValues.Add("_command", () => (IVoiceCommand)_serviceProvider.GetRequiredService(voiceCommand.GetType()));
        
        RawVoiceCommand command = new(
            Guid.NewGuid(),
            [voiceCommand.InvocationPhrase, ..GetAliases(voiceCommandType)],
            GetTags(voiceCommandType),
            capturedValues,
            GetDescription(voiceCommandType)
        );

        _log.CommandInterpreted(
            command,
            new(command.CapturedValues.Select(x => (x.Key, x.Value))),
            voiceCommandType.Name);
        return command;
    }

    private Dictionary<string, object> CaptureValues(object voiceCommand)
    {
        var bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                           BindingFlags.NonPublic;
        return voiceCommand.GetType()
                           .GetFields(bindingFlags)
                           .Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(voiceCommand)!))
                           .Union(voiceCommand.GetType()
                                              .GetProperties(bindingFlags)
                                              .Select(x =>
                                                  new KeyValuePair<string, object>(x.Name, x.GetValue(voiceCommand)!)))
                           .ToDictionary();
    }

    private string GetDescription(Type type)
    {
        var descriptionAttribute = type.GetCustomAttributes(false).OfType<DescriptionAttribute>().SingleOrDefault();
        return descriptionAttribute?.Description ?? string.Empty;
    }

    private TagRequirement[] GetTags(Type type)
    {
        Tag activationTag = new(GetActivationMode(type).ToString());
        var tagAttribute = type.GetCustomAttributes(false).OfType<TagAttribute>().ToArray();
        return tagAttribute.Any()
                   ? tagAttribute.Select(x => new TagRequirement([activationTag, ..x.Tags])).ToArray()
                   : [new([activationTag])];
    }

    private ActivationMode GetActivationMode(Type type)
    {
        var activationModeAttribute =
            type.GetCustomAttributes(false).OfType<ActivationModeAttribute>().SingleOrDefault();
        return activationModeAttribute?.ActivationMode ?? ActivationMode.Command;
    }

    private string[] GetAliases(Type type)
    {
        var aliasAttributes = type.GetCustomAttributes(false).OfType<AliasAttribute>();
        return aliasAttributes.SelectMany(x => x.Aliases).ToArray();
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Command ({commandTypeName}) Interpreted ({command}).\r\nValues captured: {capturedValues}")]
    public static partial void CommandInterpreted(this ILogger logger, RawVoiceCommand command,
                                                  LoggingEnumerator<(string, object)> capturedValues,
                                                  string commandTypeName);
}