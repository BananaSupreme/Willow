using Microsoft.Extensions.DependencyInjection;

using System.Reflection;
using System.Text;

using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;
using Willow.Helpers;
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
        capturedValues.Add("_command",
            () => (IVoiceCommand)_serviceProvider.GetRequiredService(voiceCommand.GetType()));

        RawVoiceCommand command = new(
            Guid.NewGuid(),
            [voiceCommand.InvocationPhrase, ..GetAliases(voiceCommandType)],
            GetTags(voiceCommandType),
            capturedValues,
            GetSupportedOperatingSystem(voiceCommandType),
            GetName(voiceCommandType),
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

    private string GetName(Type type)
    {
        var nameAttribute = type.GetCustomAttributes(false).OfType<NameAttribute>().FirstOrDefault();
        return nameAttribute?.Name ?? ProcessNameFromTypeName(type);
    }

    private static string ProcessNameFromTypeName(Type type)
    {
        var typeName = GetTypeNameWithoutEndings(type);
        return ProcessName(typeName);
    }

    private static string ProcessName(ReadOnlySpan<char> typeName)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append(typeName[0]);

        for (var i = 1; i < typeName.Length; i++)
        {
            if (char.IsUpper(typeName[i]))
            {
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(typeName[i]);
        }

        return stringBuilder.ToString();
    }

    private static ReadOnlySpan<char> GetTypeNameWithoutEndings(Type type)
    {
        const string command = "Command";
        const string voiceCommand = "VoiceCommand";
        var typeName = type.Name.AsSpan();
        if (typeName.EndsWith(voiceCommand))
        {
            typeName = typeName[..^voiceCommand.Length];
        }
        else if (typeName.EndsWith(command))
        {
            typeName = typeName[..^command.Length];
        }

        return typeName;
    }

    private string GetDescription(Type type)
    {
        var descriptionAttribute = type.GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault();
        return descriptionAttribute?.Description ?? string.Empty;
    }

    private TagRequirement[] GetTags(Type type)
    {
        Tag activationTag = new(GetActivationMode(type).ToString());
        var tagAttribute = type.GetCustomAttributes(false)
                               .OfType<TagAttribute>()
                               .ToArray();
        return tagAttribute.Any()
                   ? tagAttribute.Select(x => new TagRequirement([activationTag, ..x.Tags])).ToArray()
                   : [new([activationTag])];
    }

    private ActivationMode GetActivationMode(Type type)
    {
        var activationModeAttribute = type.GetCustomAttributes(false)
                                          .OfType<ActivationModeAttribute>()
                                          .FirstOrDefault();
        return activationModeAttribute?.ActivationMode ?? ActivationMode.Command;
    }

    private string[] GetAliases(Type type)
    {
        var aliasAttributes = type.GetCustomAttributes(false).OfType<AliasAttribute>();
        return aliasAttributes.SelectMany(x => x.Aliases).ToArray();
    }

    private SupportedOperatingSystems GetSupportedOperatingSystem(Type type)
    {
        var supportsOperatingSystemAttribute = type.GetCustomAttributes(false)
                                                   .OfType<SupportedOperatingSystemsAttribute>()
                                                   .FirstOrDefault();
        return supportsOperatingSystemAttribute?.SupportedOperatingSystems ?? SupportedOperatingSystems.All;
    }
}

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Command ({commandTypeName}) Interpreted ({command}).\r\nValues captured: {capturedValues}")]
    public static partial void CommandInterpreted(this ILogger logger, RawVoiceCommand command,
                                                  EnumeratorLogger<(string, object)> capturedValues,
                                                  string commandTypeName);
}