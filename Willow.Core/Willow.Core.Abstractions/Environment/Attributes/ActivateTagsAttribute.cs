namespace Willow.Environment.Attributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ActivateTagsAttribute : Attribute
{
    //GUIDE_REQUIRED ENVIRONMENT TAGS
    /// <summary>
    /// This is scanned when the an assembly is loaded and is the way to add tags that should be associated to a process.
    /// </summary>
    /// <example>
    /// <code>
    /// [assembly: ActivateTags("rider", "IDE", "code editor")]
    /// </code>
    /// </example>
    /// <remarks>
    /// When multiple tags are associated with the same process they become additive. for example
    /// <code>
    /// [assembly: ActivateTags("vscode", "code editor")]
    /// [assembly: ActivateTags("vscode", "text editor")]
    /// </code>
    /// this will result in both <b>"code editor"</b> and <b>"test editor"</b> becoming active when the user views vscode.
    /// </remarks>
    /// <param name="processName">The process name that is associated to the tags.</param>
    /// <param name="tags">The tags that are activated when the process triggers.</param>
    public ActivateTagsAttribute(string processName, params string[] tags)
    {
        ProcessName = processName;
        Tags = tags;
    }

    public string ProcessName { get; }
    public string[] Tags { get; }
}
