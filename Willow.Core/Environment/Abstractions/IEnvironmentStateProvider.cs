using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.Abstractions;

/// <summary>
/// Reports about the state of the current environment.
/// </summary>
public interface IEnvironmentStateProvider
{
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
    void SetActivationMode(ActivationMode activationMode);

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
    /// using the <see cref="DeactivateTag"/> function.
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