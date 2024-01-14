using Willow.Speech.Microphone.Models;
using Willow.Speech.Microphone.Registration;

namespace Willow.Speech.Microphone.Eventing.Events;

/// <summary>
/// Captured <see cref="AudioData"/> from microphone, currently this data reaches the events after being processed.
/// </summary>
/// <remarks>
/// Interceptors for this event are registered at <see cref="AudioCapturedEventInterceptorRegistrar"/>
/// </remarks>
/// <param name="AudioData">The captured data.</param>
public readonly record struct AudioCapturedEvent(AudioData AudioData);