using Willow.Speech.Microphone.Models;
using Willow.Speech.VAD.Models;

namespace Willow.Speech.VAD.Abstractions;

/// <summary>
/// Enables voice activity detection in the system, that is, this system identifies when the user actually speaks.
/// </summary>
internal interface IVoiceActivityDetection
{
    /// <summary>
    /// Detects speech within the audio provided.
    /// </summary>
    /// <param name="audioSegment">The audio data to detect speech within.</param>
    /// <returns>The result of the detection.</returns>
    VoiceActivityResult Detect(AudioData audioSegment);
}