using Willow.Environment.Enums;
using Willow.Environment.Models;

namespace Willow.Environment;

//GUIDE_REQUIRED EXPLAIN ABOUT TAGS IN THE SYSTEM
/// <summary>
/// Reports about the state of the current environment.
/// </summary>
public interface IEnvironmentStateProvider
{
    /// <summary>
    /// The default activation mode in the system, basically command mode.
    /// </summary>
    public const string DefaultActivationMode = "command";

    /// <summary>
    /// All the currently active tags.
    /// </summary>
    IReadOnlyList<Tag> Tags { get; }

    /// <summary>
    /// The current OS.
    /// </summary>
    SupportedOss ActiveOs { get; }

    /// <summary>
    /// Sets the currently active window.
    /// </summary>
    /// <param name="activeWindow">The active window info.</param>
    internal void SetActiveWindowInfo(ActiveWindowInfo activeWindow);

    /// <summary>
    /// Sets the tag associated with active window.
    /// </summary>
    /// <param name="tags">The tags associated with the current windows, internal as it is not relevant for command creation.</param>
    internal void SetWindowTags(Tag[] tags);

    /// <summary>
    /// Sets the current activation mode in the system.
    /// </summary>
    /// <param name="activationMode">Current activation mode.</param>
    void SetActivationMode(string activationMode);

    /// <summary>
    /// Activates an arbitrary tag in the system. <br/>
    /// This is the primary function command developers should use when they need to trigger commands that are
    /// only available when other commands are triggered.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If a tag is already active, this is silently ignored.
    /// </para>
    /// <para>
    /// Command developers should ensure to deactivate tags when they are no longer needed
    /// using the <see cref="DeactivateTag" /> function.
    /// </para>
    /// </remarks>
    /// <param name="tag">The tag to activate.</param>
    void ActivateTag(Tag tag);

    /// <summary>
    /// Deactivates an arbitrary tag in the system.<br/>
    /// This deactivates a previously activated tag,
    /// and should be used to clean up tags after the command no longer needs them.
    /// </summary>
    /// <remarks>
    /// If a tag is not found, the system silently ignores the function.
    /// </remarks>
    /// <param name="tag">The tag to deactivate.</param>
    void DeactivateTag(Tag tag);
}
