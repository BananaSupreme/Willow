namespace Willow.Helpers.Logging.Loggers;

/// <summary>
/// Wrapper around <typeparamref name="T" /> that returns the type name.
/// </summary>
public readonly struct TypeNameLogger<T>
{
    private readonly T _item;

    public TypeNameLogger(T item)
    {
        _item = item;
    }

    public override string ToString()
    {
        return _item is null ? $"_item of type ({typeof(T)} was resolved as null here)" : _item.GetType().ToString();
    }
}
