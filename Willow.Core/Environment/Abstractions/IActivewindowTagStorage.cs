using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.Abstractions;

/// <summary>
/// A storage for discovered tags in the assemblies registered with Willow.
/// </summary>
internal interface IActiveWindowTagStorage
{
    /// <summary>
    /// Get tags relevant to the process.
    /// </summary>
    /// <param name="processName">The name of the process the tags are bound to.</param>
    /// <returns>Tags associated to the process.</returns>
    public Tag[] GetByProcessName(string processName);

    /// <summary>
    /// Adds the tags to storage.
    /// </summary>
    /// <param name="tags">The mappings of the tags.</param>
    public void Add(IDictionary<string, Tag[]> tags);

    /// <summary>
    /// Removes the tags from storage.
    /// </summary>
    /// <param name="tags">The mappings of the tags.</param>
    public void Remove(IDictionary<string, Tag[]> tags);
}
