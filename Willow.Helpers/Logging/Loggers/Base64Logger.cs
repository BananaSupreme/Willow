using System.Buffers.Text;
using System.Text;

namespace Willow.Helpers.Logging.Loggers;

public readonly struct Base64Logger
{
    private readonly ReadOnlyMemory<byte> _bytes;

    public Base64Logger(ReadOnlyMemory<byte> bytes)
    {
        _bytes = bytes;
    }

    public override string ToString()
    {
        Span<byte> span = stackalloc byte[_bytes.Length];
        _ = Base64.EncodeToUtf8(
            _bytes.Span, 
            span, 
            out _, 
            out var written);

        return Encoding.UTF8.GetString(span[..written]);
    }

    public static implicit operator Base64Logger(byte[] input)
    {
        return new(input);
    }
}