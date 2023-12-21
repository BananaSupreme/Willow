namespace Willow.Core.Helpers.Logging;

internal readonly struct LoggingEnumerator<T>
{
    private readonly IEnumerable<T> _internalEnumerable;

    public LoggingEnumerator(IEnumerable<T> internalEnumerable)
    {
        _internalEnumerable = internalEnumerable;
    }

    public override string ToString()
    {
        return string.Join(System.Environment.NewLine, _internalEnumerable);
    }
}