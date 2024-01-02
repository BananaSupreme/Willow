using System.Text.Json;

// ReSharper disable MemberCanBePrivate.Global

namespace Willow.Helpers.Logging.Loggers;

public readonly struct JsonLogger<T>
{
    private readonly T? _item;

    public JsonLogger(T? item)
    {
        _item = item;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(_item);
    }

    public static implicit operator JsonLogger<T>(T? item)
    {
        return new(item);
    }
}