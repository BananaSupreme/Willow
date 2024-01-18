using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;
using Willow.Helpers.Extensions;
using Willow.Helpers.Logging.Loggers;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface;

internal sealed class VoiceCommandInterpreter : IVoiceCommandInterpreter
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

        RawVoiceCommand command = new(Guid.NewGuid(),
                                      [voiceCommand.InvocationPhrase, ..GetAliases(voiceCommandType)],
                                      GetTags(voiceCommandType),
                                      capturedValues,
                                      GetSupportedOs(voiceCommandType),
                                      GetName(voiceCommandType),
                                      GetDescription(voiceCommandType));

        _log.CommandInterpreted(command,
                                new EnumeratorLogger<(string, object)>(
                                    command.CapturedValues.Select(static x => (x.Key, x.Value))),
                                voiceCommandType.Name);
        return command;
    }

    private static Dictionary<string, object> CaptureValues(object voiceCommand)
    {
        const BindingFlags BindingFlags
            = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        return voiceCommand.GetType()
                           .GetFields(BindingFlags)
                           .Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(voiceCommand)!))
                           .Union(voiceCommand.GetType()
                                              .GetProperties(BindingFlags)
                                              .Select(x => new KeyValuePair<string, object>(
                                                          x.Name,
                                                          x.GetValue(voiceCommand)!)))
                           .ToDictionary();
    }

    private static string GetName(Type type)
    {
        var nameAttribute = type.GetCustomAttributes(false).OfType<NameAttribute>().FirstOrDefault();
        return nameAttribute?.Name ?? ProcessNameFromTypeName(type);
    }

    private static string ProcessNameFromTypeName(Type type)
    {
        var typeName = GetTypeNameWithoutEndings(type);
        return typeName.GetTitleFromPascal().ToString();
    }

    private static ReadOnlySpan<char> GetTypeNameWithoutEndings(Type type)
    {
        const string Command = "Command";
        const string VoiceCommand = "VoiceCommand";
        var typeName = type.Name.AsSpan();
        if (typeName.EndsWith(VoiceCommand))
        {
            typeName = typeName[..^VoiceCommand.Length];
        }
        else if (typeName.EndsWith(Command))
        {
            typeName = typeName[..^Command.Length];
        }

        return typeName;
    }

    private static string GetDescription(Type type)
    {
        var descriptionAttribute = type.GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault();
        return descriptionAttribute?.Description ?? string.Empty;
    }

    private static TagRequirement[] GetTags(Type type)
    {
        Tag activationTag = new(GetActivationMode(type).ToString());
        var tagAttribute = type.GetCustomAttributes(false).OfType<TagAttribute>().ToArray();
        return tagAttribute.Any()
                   ? tagAttribute.Select(x => new TagRequirement([activationTag, ..x.Tags])).ToArray()
                   : [new TagRequirement([activationTag])];
    }

    private static ActivationMode GetActivationMode(Type type)
    {
        var activationModeAttribute = type.GetCustomAttributes(false).OfType<ActivationModeAttribute>().FirstOrDefault();
        return activationModeAttribute?.ActivationMode ?? ActivationMode.Command;
    }

    private static string[] GetAliases(Type type)
    {
        var aliasAttributes = type.GetCustomAttributes(false).OfType<AliasAttribute>();
        return aliasAttributes.SelectMany(static x => x.Aliases).ToArray();
    }

    private static SupportedOss GetSupportedOs(Type type)
    {
        var supportsOsAttribute = type.GetCustomAttributes(false).OfType<SupportedOssAttribute>().FirstOrDefault();
        return supportsOsAttribute?.SupportedOss ?? SupportedOss.All;
    }
}

internal static partial class VoiceCommandInterpreterLoggingExtensions
{
    [LoggerMessage(EventId = 1,
                   Level = LogLevel.Debug,
                   Message
                       = "Command ({commandTypeName}) Interpreted ({command}).\r\nValues captured: {capturedValues}")]
    public static partial void CommandInterpreted(this ILogger logger,
                                                  RawVoiceCommand command,
                                                  EnumeratorLogger<(string, object)> capturedValues,
                                                  string commandTypeName);
}
