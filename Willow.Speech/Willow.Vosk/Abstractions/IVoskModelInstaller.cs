using Willow.Vosk.Settings;

namespace Willow.Vosk.Abstractions;

internal interface IVoskModelInstaller
{
    Task EnsureExistsAsync();
    Task<bool> ValidateModelFilesAsync(VoskSettings settings);
}
