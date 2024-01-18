using Willow.Core.Environment.Models;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.VoiceCommandCompilation.Models;

/// <summary>
/// A subset of <see cref="RawVoiceCommand" /> including only the parameters needed for compilation.
/// </summary>
/// <param name="Id">The Id of the command.</param>
/// <param name="InvocationPhrase">
/// The phrase that trigger a command to execute, built with the Willow command syntax.
/// </param>
/// <param name="TagRequirements">The tag requirements associated with the command.</param>
/// <param name="CapturedValues">All the fields and properties, private or not defined in the voice command.</param>
/// <seealso cref="RawVoiceCommand" />
public readonly record struct PreCompiledVoiceCommand(Guid Id,
                                                      string InvocationPhrase,
                                                      TagRequirement[] TagRequirements,
                                                      Dictionary<string, object> CapturedValues);
