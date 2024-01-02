using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.Abstractions;

public interface IEnvironmentStateProvider
{
    IReadOnlyList<Tag> Tags { get; }
    
    internal string OperatingSystem { get; }
    internal string ActivationMode { get; set; }
    internal Tag[] EnvironmentTags { get; set; }
    internal ActiveWindowInfo ActiveWindow { get; set; }

    void AddTag(Tag tag);
    void RemoveTag(Tag tag);
}