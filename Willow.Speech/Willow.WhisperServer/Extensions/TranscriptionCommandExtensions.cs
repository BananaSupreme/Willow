using Willow.WhisperServer.Helpers;
using Willow.WhisperServer.Models;
using Willow.WhisperServer.Settings;

namespace Willow.WhisperServer.Extensions;

internal static class TranscriptionCommandExtensions
{
    public static string TranscribeAudio(this PyModule scope,
                                         TranscriptionParameters parameters,
                                         TranscriptionSettings settings)
    {
        var audioDataAsWav = parameters.Audio.ToWavFile();
        var audioData = audioDataAsWav.ToPython();
        dynamic model = scope.Get("model");
        dynamic io = scope.Get("io");
        // ReSharper disable once CoVariantArrayConversion
        var temperatureList = new PyList(settings.Temperature.Select(static t => new PyFloat(t)).ToArray());
        // ReSharper disable once CoVariantArrayConversion
        var suppressTokensList = new PyList(settings.SuppressTokens.Select(static t => new PyInt(t)).ToArray());

        var result = model.transcribe(audio: io.BytesIO(audioData),
                                      language: parameters.Language,
                                      task: "transcribe",
                                      beam_size: settings.BeamSize,
                                      best_of: settings.BestOf,
                                      patience: settings.Patience,
                                      length_penalty: settings.LengthPenalty,
                                      repetition_penalty: settings.RepetitionPenalty,
                                      no_repeat_ngram_size: settings.NoRepeatNgramSize,
                                      temperature: temperatureList,
                                      compression_ratio_threshold: settings.CompressionRatioThreshold,
                                      log_prob_threshold: settings.LogProbThreshold,
                                      no_speech_threshold: settings.NoSpeechThreshold,
                                      condition_on_previous_text: settings.ConditionOnPreviousText,
                                      prompt_reset_on_temperature: settings.PromptResetOnTemperature,
                                      initial_prompt: parameters.InitialPrompt,
                                      prefix: parameters.Prefix,
                                      suppress_blank: settings.SuppressBlank,
                                      suppress_tokens: suppressTokensList);

        return InterpretResult(result);
    }

    private static string InterpretResult(dynamic result)
    {
        PyTuple asTuple = PyTuple.AsTuple(result);
        var iteratorPart = asTuple.First().GetIterator();
        var iterator = new PyIterEnumerable(iteratorPart);
        var strings = iterator.Cast<dynamic>().Select(static x => ((string)x.text).ToString().Trim());
        return string.Join(" ", strings);
    }
}
