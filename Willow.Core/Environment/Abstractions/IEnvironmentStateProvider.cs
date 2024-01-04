using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.Abstractions;

public interface IEnvironmentStateProvider
{
    IReadOnlyList<Tag> Tags { get; }
    internal void SetActiveWindowInfo(ActiveWindowInfo activeWindow);
    internal void SetEnvironmentTags(Tag[] tags);
    void SetActivationMode(ActivationMode activationMode);
    void ActivateTag(Tag tag);
    void DeactivateTag(Tag tag);
}