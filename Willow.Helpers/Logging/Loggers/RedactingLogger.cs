namespace Willow.Helpers.Logging.Loggers;

public readonly struct RedactingLogger<T>
{
    private const string _redacted = "**REDACTED**";
    private readonly T _item;
    private readonly bool _allow;

    public RedactingLogger(T item, bool allow)
    {
        _item = item;
        _allow = allow;
    }

    public override string ToString()
    {
        return _allow ? _item?.ToString() ?? string.Empty : _redacted;
    }
}