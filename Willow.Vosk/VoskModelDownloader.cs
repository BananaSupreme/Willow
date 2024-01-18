using Willow.Vosk.Abstractions;
using Willow.Vosk.Enums;

namespace Willow.Vosk;

internal sealed class VoskModelDownloader : IVoskModelDownloader
{
    private readonly HttpClient _client;

    public VoskModelDownloader(HttpClient client)
    {
        _client = client;
    }

    public async Task<Stream> GetVoskModelZip(VoskModel voskModel)
    {
        return await _client.GetStreamAsync(GetDownloadPath(voskModel));
    }

    private static string GetDownloadPath(VoskModel voskModel)
    {
        var fileName = voskModel switch
        {
            VoskModel.Small => "vosk-model-small-en-us-0.15",
            VoskModel.Medium => "vosk-model-en-us-0.22-lgraph",
            VoskModel.Large => "vosk-model-en-us-0.22"
        };

        return $"https://alphacephei.com/vosk/models/{fileName}.zip";
    }
}
