using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;

namespace Willow.Core.Environment;

internal sealed class EnvironmentStateProvider : IEnvironmentStateProvider
{
    public IReadOnlyList<Tag> Tags
    {
        get
        {
            return
            [
                new(OperatingSystem),
                new(ActivationMode),
                .. EnvironmentTags,
                .. _customTags,
                new(ActiveWindow.ProcessName)
            ];
        }
    }

    public string OperatingSystem => System.Environment.OSVersion.Platform.ToString();
    public string ActivationMode { get; set; } = nameof(Enums.ActivationMode.Command);
    public Tag[] EnvironmentTags { get; set; } = [];
    public ActiveWindowInfo ActiveWindow { get; set; } = new(string.Empty);

    private readonly HashSet<Tag> _customTags = [];

    public void AddTag(Tag tag)
    {
        _customTags.Add(tag);
    }

    public void RemoveTag(Tag tag)
    {
        _customTags.Remove(tag);
    }
}