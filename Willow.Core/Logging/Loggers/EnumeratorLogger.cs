namespace Willow.Core.Logging.Loggers;

internal readonly struct EnumeratorLogger<T>
{
    private readonly IEnumerable<T> _internalEnumerable;

    public EnumeratorLogger(IEnumerable<T> internalEnumerable)
    {
        _internalEnumerable = internalEnumerable;
    }

    public override string ToString()
    {
        return string.Join(System.Environment.NewLine, _internalEnumerable);
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