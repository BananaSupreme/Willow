using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing.Abstractions;

namespace Willow.Speech.VoiceCommandCompilation.Abstractions;

/// <summary>
/// The access point and creation of the <see cref="ITrie" />.
/// </summary>
internal interface ITrieFactory
{
    /// <summary>
    /// Rebuilds the internal <see cref="ITrie" />.
    /// </summary>
    /// <param name="commands">The commands that should be included in the tree.</param>
    void Set(PreCompiledVoiceCommand[] commands);

    /// <summary>
    /// Get the last built <see cref="ITrie" /> in the system.
    /// </summary>
    /// <returns>The last <see cref="ITrie" /> cached in the system.</returns>
    ITrie? Get();
}
