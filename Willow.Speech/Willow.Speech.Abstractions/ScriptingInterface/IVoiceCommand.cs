using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Willow.Speech.ScriptingInterface;

//GUIDE_REQUIRED
/// <summary>
/// This interface defines voice commands in the system.
/// <list type="bullet">
/// <item>
/// Commands at a minimum contain 2 components, a phrase that is invoked and constructed with our command
/// syntax and
/// A function that is executed that contains the execution context of the system.
/// </item>
/// <item>
/// Along side those functions additional attributes can decorate the command.
/// </item>
/// <item>
/// All private fields and properties are captured into <see cref="RawVoiceCommand" />
/// <b><i>CapturedValues</i></b>
/// property
/// registering their value by name for use by the node compilers, such as OneOfNodeCompiler.
/// <br/>
/// this is only done at capture time, so those collections cannot be dynamic.
/// </item>
/// <item>
/// The command is created via DI, so dependencies can be loaded using the constructor if they are available
/// for
/// injection.
/// </item>
/// <item>
/// All the commands are registered as singleton and do not include any outer synchronization mechanism, it is
/// up to
/// the implementation to ensure thread-safety.
/// </item>
/// <item>
/// Lastly commands are discovered automatically in all assemblies registered into Willow.<br/>
/// </item>
/// </list>
/// For in-depth conversation and guides check out our documentation.
/// </summary>
/// <seealso cref="ActivationModeAttribute" />
/// <seealso cref="AliasAttribute" />
/// <seealso cref="DescriptionAttribute" />
/// <seealso cref="NameAttribute" />
/// <seealso cref="SupportedOssAttribute" />
/// <seealso cref="TagAttribute" />
public interface IVoiceCommand
{
    internal const string CommandFunctionName = "_command";

    /// <summary>
    /// The primary invocation phrase to associate with this command.
    /// </summary>
    /// <example>
    /// <c>[start|stop]:activation command</c>
    /// </example>
    string InvocationPhrase { get; }

    /// <summary>
    /// This task gets executed when the command is activated.
    /// </summary>
    /// <remarks>
    /// Be aware that the system guarantees that command triggered with captured values as requested in the invocation
    /// phrases, so implementors should plan for cases where they are missing.
    /// </remarks>
    /// <param name="context">The context of the current execution.</param>
    Task ExecuteAsync(VoiceCommandContext context);
}
