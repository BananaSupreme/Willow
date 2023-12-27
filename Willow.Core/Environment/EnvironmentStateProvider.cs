using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;

namespace Willow.Core.Environment;

internal class EnvironmentStateProvider : IEnvironmentStateProvider
{
    public IReadOnlyList<Tag> Tags
    {
        get
        {
            return
            [
                new Tag(OperatingSystem), 
                new Tag(ActivationMode), 
                .. EnvironmentTags,
                new Tag(ActiveWindow.ProcessName)
            ];
        }
    }

    public string OperatingSystem => System.Environment.OSVersion.Platform.ToString();
    public string ActivationMode { get; set; } = nameof(Enums.ActivationMode.Command);
    public Tag[] EnvironmentTags { get; set; } = [];
    public ActiveWindowInfo ActiveWindow { get; set; } = new(string.Empty);
}