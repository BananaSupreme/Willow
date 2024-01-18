using System.Text.Json;

namespace Willow.Helpers.Logging.Loggers;

/// <summary>
/// A wrapper around <typeparamref name="T" /> that outputs its <i>JSON</i> representation when <see cref="ToString" />
/// .
/// is called
/// </summary>
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
        return new JsonLogger<T>(item);
    }
}
