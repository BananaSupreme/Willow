using Willow.Speech.Microphone.Models;

namespace Willow.Speech.Microphone.Eventing.Events;

public readonly record struct AudioCapturedEvent(AudioData AudioData);