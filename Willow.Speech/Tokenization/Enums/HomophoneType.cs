namespace Willow.Speech.Tokenization.Enums;

/// <summary>
/// Type of homophone comparison to use.
/// </summary>
public enum HomophoneType
{
    /// <summary>
    /// Direct homophone as based on the Carnegie Melon Phonetics Dictionary.
    /// </summary>
    /// <seealso href="https://en.wikipedia.org/wiki/CMU_Pronouncing_Dictionary"/>
    CarnegieMelonDictionaryEquivalents,

    /// <summary>
    /// Near homophones as defined by being a Levenshtein distance of 1 from the other homophones as defined by the
    /// Carnegie Melon Phonetics Dictionary.
    /// </summary>
    /// <seealso href="https://en.wikipedia.org/wiki/CMU_Pronouncing_Dictionary"/>
    /// <seealso href="https://www.geeksforgeeks.org/introduction-to-levenshtein-distance/"/>
    CarnegieMelonDictionaryNearEquivalents,

    /// <summary>
    /// Use Metaphone codes to compare words.
    /// </summary>
    /// <seealso href="https://en.wikipedia.org/wiki/Metaphone"/>
    Metaphone,

    /// <summary>
    /// Use Caverphone codes to compare words.
    /// </summary>
    /// <seealso href="https://en.wikipedia.org/wiki/Caverphone"/>
    Caverphone
}
