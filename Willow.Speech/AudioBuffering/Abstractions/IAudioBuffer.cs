using Willow.Speech.AudioBuffering.Exceptions;
using Willow.Speech.AudioBuffering.Settings;
using Willow.Speech.Microphone.Models;

namespace Willow.Speech.AudioBuffering.Abstractions;

/// <summary>
/// A buffer to store <see cref="AudioData"/> inside.
/// </summary>
internal interface IAudioBuffer
{
    /// <summary>
    /// True if the buffer has enough space to add <paramref name="space"/>.
    /// </summary>
    /// <param name="space">The amount of space needed.</param>
    /// <returns>True if can accommodate, otherwise false.</returns>
    bool HasSpace(int space);

    /// <summary>
    /// Tries to load data unto the buffer if it has enough space to accommodate it.
    /// </summary>
    /// <param name="audioData">The audio data to load.</param>
    /// <returns>True if loaded successfully, otherwise false.</returns>
    /// <exception cref="MismatchedFeaturesException">
    /// Throws if tries to load <see cref="AudioData"/> with different parameters from those that were originally
    /// inputted, or defined in the <see cref="AudioBufferSettings"/>.
    /// </exception>
    bool TryLoadData(AudioData audioData);

    /// <summary>
    /// Unloads all the data from the buffer.
    /// </summary>
    /// <returns>All the data in the audio buffer as a single <see cref="AudioData"/> object.</returns>
    AudioData UnloadAllData();
    
    /// <summary>
    /// Unloads as much data as requested, or everything if the buffer has less.
    /// </summary>
    /// <param name="maximumRequested">The amount requested.</param>
    /// <returns>The data requested as a single <see cref="AudioData"/> objects, and its length.</returns>
    (AudioData AudioData, int ActualSize) UnloadData(int maximumRequested);
}