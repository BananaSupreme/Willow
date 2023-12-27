using Willow.Core.Environment.Abstractions;
using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.ActiveWindowDetectors;

internal class EmptyActiveWindowDetector : IActiveWindowDetector
{
    public ActiveWindowInfo GetActiveWindow()
    {
        return new("Testing");
    }
}