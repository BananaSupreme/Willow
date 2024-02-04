using Willow.Speech.Tokenization.Enums;

namespace Willow.Speech.Tokenization.Settings;

/// <summary>
/// The type of equivalence to use when comparing words.
/// </summary>
/// <param name="ShouldTestHomophones">Whether any equivalence check should be attempted.</param>
/// <param name="HomophoneType">The type of equivalence check.</param>
/// <param name="UserDefinedHomophones">
/// Added user defined homophones, those are always used, regardless of <paramref name="ShouldTestHomophones"/>
/// </param>
public readonly record struct HomophoneSettings(bool ShouldTestHomophones,
                                                HomophoneType HomophoneType,
                                                Dictionary<string, string[]> UserDefinedHomophones)
{
    public HomophoneSettings() : this(true, HomophoneType.CarnegieMelonDictionaryEquivalents, [])
    {
    }
}
