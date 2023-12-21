namespace Willow.Core.Environment.Models;

public readonly record struct ActiveWindowInfo(
    string Title,
    int ProcessId,
    string ProcessName);