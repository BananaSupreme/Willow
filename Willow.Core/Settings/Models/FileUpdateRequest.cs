namespace Willow.Core.Settings.Models;

/// <summary>
/// A file writing request to <see cref="IQueuedFileWriter"/>.
/// </summary>
/// <param name="File">The file to write onto.</param>
/// <param name="Value">The <i>JSON</i> representation of the object.</param>
internal readonly record struct FileUpdateRequest(FileStream File, string Value);