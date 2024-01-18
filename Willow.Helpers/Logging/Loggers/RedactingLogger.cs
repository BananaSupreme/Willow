namespace Willow.Helpers.Logging.Loggers;

/// <summary>
/// Wrapper around <typeparamref name="T" /> where the output is redacted when the incoming boolean is false. <br/>
/// This is the primary way logging sensitive information should be accomplished.
/// </summary>
public readonly struct RedactingLogger<T>
{
    private const string Redacted = "**REDACTED**";
    private readonly T _item;
    private readonly bool _allow;

    public RedactingLogger(T item, bool allow)
    {
        _item = item;
        _allow = allow;
    }

    public override string ToString()
    {
        return _allow ? _item?.ToString() ?? string.Empty : Redacted;
    }
}
