using System.Collections.Frozen;
using System.Text.Json;

using Willow.Core.Settings.Abstractions;
using Willow.Speech.Tokenization.Abstractions;
using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Settings;

namespace Tests.Helpers;

//This is really similar to the actual homophones loader, but its here so the tests would still work even if we change
//slightly the dictionaries and how they are built.
internal sealed class HomophoneDictionaryLoaderTestDouble : IHomophonesDictionaryLoader
{
    private readonly ISettings<HomophoneSettings> _settings;

    public HomophoneDictionaryLoaderTestDouble(ISettings<HomophoneSettings> settings)
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
                                 ? "homophones.dict"
                                 : "near_homophones.dict";
        var path = Path.Combine("Speech", "CommandProcessing", dictionaryName);
        await using var file = File.OpenRead(path);
        var homophones = await JsonSerializer.DeserializeAsync<Dictionary<string, string[]>>(file)
                         ?? throw new InvalidOperationException();
        return homophones.ToFrozenDictionary();
    }
}
