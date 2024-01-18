using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;
using Willow.Speech.ScriptingInterface.Abstractions;

namespace Willow.Speech.ScriptingInterface.Models;

/// <summary>
/// The parsed result of the <see cref="IVoiceCommand" /> implementors to be used within the system.
/// </summary>
/// <param name="Id">The Id of the command.</param>
/// <param name="InvocationPhrases">
/// The phrases that trigger a command to execute, built with the Willow command syntax.
/// </param>
/// <param name="TagRequirements">The tag requirements associated with the command.</param>
/// <param name="CapturedValues">All the fields and properties, private or not defined in the voice command.</param>
/// <param name="SupportedOss">The operating systems this command supports.</param>
/// <param name="Name">The name of the command.</param>
/// <param name="Description">A human readable description of the command.</param>
/// <seealso cref="IVoiceCommand" />
public readonly record struct RawVoiceCommand(Guid Id,
                                              string[] InvocationPhrases,
                                              TagRequirement[] TagRequirements,
                                              Dictionary<string, object> CapturedValues,
                                              SupportedOss SupportedOss,
                                              string Name,
                                              string Description);
