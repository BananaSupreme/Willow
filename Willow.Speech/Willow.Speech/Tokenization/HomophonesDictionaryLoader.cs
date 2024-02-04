using System.Collections.Frozen;
using System.IO.Compression;
using System.Text.Json;

using Willow.Settings;
using Willow.Speech.Tokenization.Abstractions;
using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Settings;

namespace Willow.Speech.Tokenization;

internal sealed class HomophonesDictionaryLoader : IHomophonesDictionaryLoader
{
    private readonly ISettings<HomophoneSettings> _settings;

    public HomophonesDictionaryLoader(ISettings<HomophoneSettings> settings)
    {
        _settings = settings;
    }

    public async Task<FrozenDictionary<string, string[]>?> LoadDictionaryAsync()
    {
        if (!_settings.CurrentValue.ShouldTestHomophones
            || _settings.CurrentValue.HomophoneType is not (HomophoneType.CarnegieMelonDictionaryEquivalents
                or HomophoneType.CarnegieMelonDictionaryNearEquivalents))
        {
            return null;
        }

        var dictionaryName = _settings.CurrentValue.HomophoneType == HomophoneType.CarnegieMelonDictionaryEquivalents
                                 ? "homophones.brotli"
                                 : "near_homophones.brotli";
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Tokenization", "Resources", dictionaryName);
        await using var file = File.OpenRead(path);
        await using var brotli = new BrotliStream(file, CompressionMode.Decompress);
        var homophones = await JsonSerializer.DeserializeAsync<Dictionary<string, string[]>>(brotli)
                         ?? throw new InvalidOperationException();
        return homophones.ToFrozenDictionary();
    }
}
