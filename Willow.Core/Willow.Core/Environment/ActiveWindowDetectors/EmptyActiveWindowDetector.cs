using Willow.Environment.Abstractions;
using Willow.Environment.Models;

namespace Willow.Environment.ActiveWindowDetectors;

internal sealed class EmptyActiveWindowDetector : IActiveWindowDetector
{
    public ActiveWindowInfo GetActiveWindow()
    {
        return new ActiveWindowInfo("Testing");
    }
}
