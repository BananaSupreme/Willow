using Willow.Environment.Enums;
using Willow.Speech.Microphone.Models;

namespace Willow.Speech.SpeechToText;

//GUIDE_REQUIRED NEW SPEECH ENGINES.
/// <summary>
/// Defines a speech to text engine that can be used by the system.
/// </summary>
/// <remarks>
/// The system guarantees that only one engine is active at every single time.
/// </remarks>
public interface ISpeechToTextEngine
{
    /// <summary>
    /// The name of the engine.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Is this the currently running engine.
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// The OS environments this engine supports.
    /// </summary>
    public SupportedOss SupportedOss { get; }

    /// <summary>
    /// Transcribes a piece of audio and extracts the relevant text out of.
    /// </summary>
    /// <param name="audioData">The input audio.</param>
    /// <returns>The text found in the audio.</returns>
    public Task<string> TranscribeAudioAsync(AudioData audioData);

    /// <summary>
    /// This is called whenever the engine starts.
    /// </summary>
    /// <remarks>
    /// <b><i>The Start and Stop functions should completely clean up and create their own state as we do not want
    /// left over state when the user has changed the engine.
    /// </i></b>
    /// </remarks>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// This is called whenever the engine stops.
    /// </summary>
    /// <remarks>
    /// <b><i>The Start and Stop functions should completely clean up and create their own state as we do not want
    /// left over state when the user has changed the engine.
    /// </i></b>
    /// </remarks>
    Task StopAsync();
}
