using Willow.Speech.Microphone.Models;

namespace Willow.Speech.Microphone.Events;

/// <summary>
/// Captured <see cref="AudioData" /> from microphone, currently this data reaches the events after being processed.
/// </summary>
/// <param name="AudioData">The captured data.</param>
public readonly record struct AudioCapturedEvent(AudioData AudioData);
