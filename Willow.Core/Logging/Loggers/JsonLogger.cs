using System.Text.Json;

namespace Willow.Core.Logging.Loggers;

public class JsonLogger<T>
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