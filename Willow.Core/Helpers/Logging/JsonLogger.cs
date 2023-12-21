using System.Text.Json;

namespace Willow.Core.Helpers.Logging;

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
}