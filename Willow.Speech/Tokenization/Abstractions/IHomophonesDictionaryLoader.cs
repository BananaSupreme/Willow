using System.Collections.Frozen;

namespace Willow.Speech.Tokenization.Abstractions;

internal interface IHomophonesDictionaryLoader
{
    Task<FrozenDictionary<string, string[]>?> LoadDictionaryAsync();
}
