using Willow.Speech.Microphone.Models;
using Willow.Speech.Microphone.Settings;

namespace Willow.Speech.Microphone.Abstractions;

/// <summary>
/// Accessor to the users selected microphone
/// </summary>
internal interface IMicrophoneAccess
{
    /// <summary>
    /// Sets up the recording device and returns an infinite enumeration of recording data as they become available.
    /// </summary>
    /// <remarks>
    /// Changes to the microphone settings are handled internally by listening to the event fired when
    /// <see cref="MicrophoneSettings" /> changes.
    /// </remarks>
    /// <returns>An infinite stream of <see cref="AudioData" />.</returns>
    IEnumerable<AudioData> StartRecording();

    /// <summary>
    /// Stops the recording and clears up the resources.
    /// </summary>
    void StopRecording();
}
