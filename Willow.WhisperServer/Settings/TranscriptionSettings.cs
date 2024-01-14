namespace Willow.WhisperServer.Settings;

/// <summary>
/// Defines settings for transcription operations, including parameters for beam search, sampling, and penalties.
/// </summary>
/// <param name="BeamSize">
/// The beam size used in decoding, determining the number of hypotheses to be considered in parallel.
/// In simpler terms, <paramref name="BeamSize"/> refers to how many different possibilities the system considers at
/// each step of transcribing speech to text. A larger beam size means the system looks at more potential words or
/// phrases as the next part of the transcript.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="BeamSize"/></i></b>: leads to higher accuracy as more options are evaluated, but it 
/// also increases computational demands, making the process slower.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="BeamSize"/></i></b>: Makes the transcription faster and less resource-intensive, but it
/// may reduce accuracy since fewer options are considered.
/// </item>
/// </list>
/// </param>
/// <param name="BestOf">
/// The number of best candidates to consider when sampling with a non-zero temperature.
/// To put it simply, <paramref name="BestOf"/> determines how many top options the system will consider when deciding
/// what the next word in the transcript might be. Imagine it like having several top guesses from which the final
/// choice is made. Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="BestOf"/></i></b>: Allows the system to consider more options, potentially increasing
/// the accuracy of the transcription. However, this can also slow down the process due to the additional calculations
/// required.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="BestOf"/></i></b>: Speeds up the transcription process by reducing the number of
/// options considered, but it might lead to less accurate results if the correct option is not among those top few
/// considered.
/// </item>
/// </list>
/// </param>
/// <param name="Patience">
/// The patience factor for beam search, affecting the termination condition of the search.
/// In simpler terms, <paramref name="Patience"/> is like a measure of how long the system will keep looking for better
/// transcription options before deciding. A higher patience level means the system will spend more time considering
/// different possibilities.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="Patience"/></i></b>: This leads to potentially more accurate transcriptions, as the
/// system exhaustively checks more options. However, it also means longer processing times, as the system takes longer
/// to finalize its decision.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="Patience"/></i></b>: Results in quicker transcription decisions, making the process
/// faster. However, this might come at the cost of accuracy, as the system may not explore enough options before making
/// a decision.
/// </item>
/// </list>
/// </param>
/// <param name="LengthPenalty">
/// The exponential penalty constant applied to longer sequences to balance against shorter ones.
/// Simply put, <paramref name="LengthPenalty"/> is used to adjust the likelihood of longer versus shorter phrases in
/// the transcription. It helps the system not to favor either too short or too lengthy sequences unduly.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="LengthPenalty"/></i></b>: This will discourage the system from choosing longer
/// sequences, potentially making the transcription more concise. However, it might omit necessary details or context
/// by favoring brevity.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="LengthPenalty"/></i></b>: Allows for longer phrases in the transcription, which can
/// capture more detailed information. But this might lead to unnecessarily long or verbose transcriptions.
/// </item>
/// </list>
/// </param>
/// <param name="RepetitionPenalty">
/// The penalty applied to the scores of tokens that have been previously generated, used to discourage repetitions. A
/// value greater than 1 increases this penalty.
/// Essentially, <paramref name="RepetitionPenalty"/> helps the system avoid repeating the same words or phrases. It's
/// a bit like reminding someone not to keep saying the same thing over and over.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="RepetitionPenalty"/></i></b>: This strongly discourages the system from repeating
/// words, which can make the transcription more diverse and less redundant. However, it might inadvertently avoid
/// repeating words that are actually important or relevant in the context.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="RepetitionPenalty"/></i></b>: With a lower penalty, the system is less strict about
/// avoiding repetitions, which might be necessary in certain contexts. But, this could also lead to transcriptions
/// that are repetitive or less varied.
/// </item>
/// </list>
/// </param>
/// <param name="NoRepeatNgramSize">
/// The size of ngrams that should not be repeated. A value of 0 disables this constraint.
/// In simpler terms, <paramref name="NoRepeatNgramSize"/> specifies the length of the word groupings that the system
/// tries to avoid repeating. For instance, if set to 3, the system will try not to repeat any identical sequence of
/// three words.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="NoRepeatNgramSize"/></i></b>: This makes the transcription more varied by preventing
/// the repetition of longer phrases. However, it might limit the system's ability to use common phrases or expressions
/// that naturally recur in speech.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="NoRepeatNgramSize"/></i></b>: Allows for more natural repetition of phrases, which
/// could be important in certain contexts. But, it may lead to transcriptions that are repetitive, especially with
/// short, common phrases.
/// </item>
/// </list>
/// </param>
/// <param name="Temperature">
/// An array of temperatures for sampling. Higher temperatures increase randomness. Successive temperatures are used
/// upon failure based on other thresholds.
/// To explain it simply, <paramref name="Temperature"/> affects how much the system is willing to take risks in its
/// choices. A higher temperature means the system is more likely to make unusual or less predictable choices, similar
/// to a person taking more creative guesses.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="Temperature"/></i></b>: Encourages more creative and varied transcriptions,
/// potentially capturing unexpected or less common interpretations of the speech. However, this can also lead to more
/// errors or odd choices in the transcription.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="Temperature"/></i></b>: Results in more conservative and likely more accurate
/// transcriptions, sticking closely to the most probable options. This can make the transcription less prone to
/// errors, but it might miss out on capturing more nuanced or less obvious aspects of the speech.
/// </item>
/// </list>
/// </param>
/// <param name="CompressionRatioThreshold">
/// The threshold for the gzip compression ratio. If the ratio exceeds this value, the sampling is considered a failure.
/// In simpler terms, <paramref name="CompressionRatioThreshold"/> is used as a measure to decide when to stop trying
/// new transcription possibilities based on how much the generated text can be compressed. A higher compression ratio
/// usually indicates repetitive or less meaningful text.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="CompressionRatioThreshold"/></i></b>: This allows the system to generate more text
/// before considering it a failure, which could be useful in capturing longer, more detailed transcriptions. However,
/// it might also result in more repetitive or less meaningful content.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="CompressionRatioThreshold"/></i></b>: The system will stop and reset sooner when
/// generating text, potentially leading to more concise and relevant transcriptions. But, this might cause it to miss
/// out on longer, yet important, parts of the speech.
/// </item>
/// </list>
/// </param>
/// <param name="LogProbThreshold">
/// The threshold for average log probability over sampled tokens. If it falls below this value, the sampling is
/// considered a failure.
/// Put simply, <paramref name="LogProbThreshold"/> sets a standard for how confident the system should be in its
/// choices. If the system's confidence in its transcription choices falls below this threshold, it stops and tries a
/// different approach.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="LogProbThreshold"/></i></b>: Requires the system to be more certain about its choices,
/// leading to potentially higher quality and more accurate transcriptions. However, this might cause the system to
/// reject valid options that are less obvious or have lower immediate probability.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="LogProbThreshold"/></i></b>: Allows the system to accept transcriptions with lower
/// confidence, which can be beneficial in ambiguous or complex speech segments. However, this could result in less
/// accurate transcriptions with more errors.
/// </item>
/// </list>
/// </param>
/// <param name="NoSpeechThreshold">
/// The threshold for determining silence. If the probability of no speech exceeds this value AND the average log
/// probability is below the `LogProbThreshold`, the segment is considered silent.
/// In simpler terms, <paramref name="NoSpeechThreshold"/> helps the system identify when there's no talking, like
/// pauses or quiet moments in the audio. If the system is quite sure (above this threshold) that there's no speech,
/// it will treat that part of the audio as silence.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="NoSpeechThreshold"/></i></b>: The system becomes more cautious in marking parts of the
/// audio as silent, which can be useful to avoid skipping over quiet or unclear speech. However, this might lead to
/// including unnecessary pauses or non-speech segments in the transcription.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="NoSpeechThreshold"/></i></b>: Makes the system more aggressive in identifying silence,
/// potentially leading to a cleaner transcription without unnecessary breaks. But, this may result in missing out on
/// soft-spoken words or phrases.
/// </item>
/// </list>
/// </param>
/// <param name="ConditionOnPreviousText">
/// Indicates whether the model should condition on the previous output for the next window. Disabling this may result
/// in less consistency but prevents the model from repetitive loops.
/// Put in simpler terms, <paramref name="ConditionOnPreviousText"/> determines if the system should use its previous
/// transcriptions as a reference for continuing the transcription. It's like deciding whether to look back at what's
/// already been said to maintain context in an ongoing conversation.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>True for <paramref name="ConditionOnPreviousText"/></i></b>: Helps maintain a consistent and contextually
/// accurate transcription by building on what has already been transcribed. However, this might cause the system to
/// get stuck in a loop, repeating or dwelling too much on previous content.
/// </item>
/// <item>
/// <b><i>False for <paramref name="ConditionOnPreviousText"/></i></b>: Makes the system treat each segment more
/// independently, potentially leading to a transcription with less repetitive loops. But this can result in losing
/// context and continuity between segments.
/// </item>
/// </list>
/// </param>
/// <param name="PromptResetOnTemperature">
/// Resets the prompt if the temperature is above this value. This parameter only has an effect if
/// <paramref name="ConditionOnPreviousText"/> is true.
/// To explain it simply, <paramref name="PromptResetOnTemperature"/> decides when to 'start fresh' with the
/// transcription process if the system is making more creative or less predictable choices (high temperature).
/// It's like choosing to forget the previous context and start anew if things are getting too unconventional.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Higher <paramref name="PromptResetOnTemperature"/></i></b>: Allows the system to experiment more with its
/// transcriptions before resetting. This can lead to more diverse and potentially insightful transcripts but might
/// result in straying too far from the original context.
/// </item>
/// <item>
/// <b><i>Lower <paramref name="PromptResetOnTemperature"/></i></b>: Triggers a reset more quickly when the system
/// starts taking creative liberties, keeping the transcription closely tied to the established context. However, this
/// may limit the system's ability to capture unique or nuanced aspects of the speech.
/// </item>
/// </list>
/// </param>
/// <param name="SuppressBlank">
/// Indicates whether to suppress blank outputs at the beginning of the sampling process.
/// In simpler terms, <paramref name="SuppressBlank"/> is used to decide if the system should ignore silent or empty
/// parts at the start of the audio. It's like skipping over the quiet before someone starts speaking, to get right to
/// the words.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>True for <paramref name="SuppressBlank"/></i></b>: This setting helps to start the transcription more
/// cleanly, without including unnecessary silence or pauses at the beginning. It makes the transcript tidier but
/// might skip over important brief sounds or quiet speech.
/// </item>
/// <item>
/// <b><i>False for <paramref name="SuppressBlank"/></i></b>: By not suppressing blanks, the system captures
/// everything from the very start, including any silence or noise. This can provide a complete record but may include
/// irrelevant sections before the actual speech begins.
/// </item>
/// </list>
/// </param>
/// <param name="SuppressTokens">
/// An array of token IDs to suppress. A value of -1 suppresses a default set of symbols as defined in the model's
/// configuration.
/// Essentially, <paramref name="SuppressTokens"/> allows you to specify certain words or sounds (tokens) that the
/// transcription system should ignore. It's like telling the system to pay no attention to certain repetitive or
/// irrelevant words or noises in the audio.
/// Trade-offs include:
/// <list type="bullet">
/// <item>
/// <b><i>Specifying specific tokens in <paramref name="SuppressTokens"/></i></b>: This can help in creating a cleaner,
/// more focused transcription by leaving out unnecessary elements. However, over-suppression might result in missing
/// some relevant information or nuances in the speech.
/// </item>
/// <item>
/// <b><i>Not specifying or using a default set (-1) for <paramref name="SuppressTokens"/></i></b>: Leads to a more
/// comprehensive transcription, capturing a wider range of sounds and words. But this can include unwanted noise or
/// irrelevant words, potentially cluttering the transcript.
/// </item>
/// </list>
/// </param>
public readonly record struct TranscriptionSettings(
    int BeamSize,
    int BestOf,
    float Patience,
    float LengthPenalty,
    float RepetitionPenalty,
    int NoRepeatNgramSize,
    float[] Temperature,
    float CompressionRatioThreshold,
    float LogProbThreshold,
    float NoSpeechThreshold,
    bool ConditionOnPreviousText,
    float PromptResetOnTemperature,
    bool SuppressBlank,
    int[] SuppressTokens)
{
    private static readonly float[] _temperatureDefaults = [0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f];
    private static readonly int[] _suppressTokensDefaults = [-1];

    /// <inheritdoc cref="TranscriptionSettings"/>
    public TranscriptionSettings()
        : this(
            5,
            5,
            1.0f,
            1.0f,
            1.0f,
            0,
            _temperatureDefaults,
            2.4f,
            -1.0f,
            0.7f,
            false,
            0.5f,
            false,
            _suppressTokensDefaults)
    {
    }
}