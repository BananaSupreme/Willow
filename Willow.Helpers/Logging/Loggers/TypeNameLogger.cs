using Willow.Helpers.Extensions;

namespace Willow.Helpers.Logging.Loggers;

public readonly struct TypeNameLogger<T>
{
    private readonly T _item;

    public TypeNameLogger(T item)
    {
        _item = item;
    }

    public override string ToString()
    {
        if (_item is null)
        {
            return $"_item of type ({TypeExtensions.GetFullName<T>()} was resolved as null here)";
        }
        return TypeExtensions.GetFullName(_item.GetType());
    }
}