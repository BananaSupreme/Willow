using Willow.Vosk.Enums;

namespace Willow.Vosk.Abstractions;

internal interface IVoskModelDownloader
{
    Task<Stream> GetVoskModelZip(VoskModel voskModel);
}
