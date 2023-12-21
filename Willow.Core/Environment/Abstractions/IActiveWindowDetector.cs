using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.Abstractions;

public interface IActiveWindowDetector
{
    ActiveWindowInfo GetActiveWindow();
}