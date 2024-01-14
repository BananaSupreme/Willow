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
    /// Sets the tag dictionary.
    /// </summary>
    /// <remarks>THIS IS A TEMPORARY SOLUTION AND WILL CHANGE.</remarks>
    public void Set(IDictionary<string, Tag[]> tags);
}