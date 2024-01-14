namespace Willow.Helpers.Logging.Loggers;

/// <summary>
/// Wraps an enumerable and returns a string representation of each of its members when <see cref="ToString"/> is called.
/// </summary>
public readonly struct EnumeratorLogger<T>
{
    private readonly IEnumerable<T> _internalEnumerable;

    public EnumeratorLogger(IEnumerable<T> internalEnumerable)
    {
        _internalEnumerable = internalEnumerable;
    }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, _internalEnumerable);
    }

    public static implicit operator EnumeratorLogger<T>(List<T> item)
    {
        return new(item);
    }

    public static implicit operator EnumeratorLogger<T>(T[] item)
    {
        return new(item);
    }
}