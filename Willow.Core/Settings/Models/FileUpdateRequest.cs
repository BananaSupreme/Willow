namespace Willow.Core.Settings.Models;

internal readonly record struct FileUpdateRequest(FileStream File, string Value);