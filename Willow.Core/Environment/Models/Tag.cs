// ReSharper disable NotAccessedPositionalProperty.Global -- that's the point.
namespace Willow.Core.Environment.Models;

/// <summary>
/// A tag in the system
/// </summary>
/// <param name="Name">The name of the tag</param>
public readonly record struct Tag(string Name);
