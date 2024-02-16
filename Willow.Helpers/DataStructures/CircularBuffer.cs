namespace Willow.Helpers.DataStructures;

public sealed class CircularBuffer<T>
{
    private readonly T[] _buffer;
    private int _head;
    private int _size;
    private int _tail;

    public CircularBuffer(int maxSpace)
    {
        _buffer = new T[maxSpace];
    }

    private int SpaceLeft => _buffer.Length - _size;
    public bool IsEmpty => _size == 0;

    public bool TryLoadData(T[] data)
    {
        if (data.Length > SpaceLeft)
        {
            return false;
        }

        var spaceUntilEndOfBuffer = _buffer.Length - _tail;

        if (spaceUntilEndOfBuffer >= data.Length)
        {
            Array.Copy(data, 0, _buffer, _tail, data.Length);
        }
        else
        {
            Array.Copy(data, 0, _buffer, _tail, spaceUntilEndOfBuffer);
            Array.Copy(data,
                       spaceUntilEndOfBuffer,
                       _buffer,
                       0,
                       data.Length - spaceUntilEndOfBuffer);
        }

        _tail = (_tail + data.Length) % _buffer.Length;
        _size += data.Length;
        return true;
    }

    public (T[] AudioData, int ActualSize) UnloadData(int maximumRequested)
    {
        if (_size == 0)
        {
            return ([], 0);
        }

        var actualSize = Math.Min(_size, maximumRequested);

        var arr = new T[actualSize];
        var spaceOccupiedUntilEndOfBuffer = _buffer.Length - _head;
        if (_head < _tail || actualSize < spaceOccupiedUntilEndOfBuffer)
        {
            Array.Copy(_buffer, _head, arr, 0, actualSize);
        }
        else
        {
            Array.Copy(_buffer, _head, arr, 0, spaceOccupiedUntilEndOfBuffer);
            Array.Copy(_buffer, 0, arr, spaceOccupiedUntilEndOfBuffer, actualSize - spaceOccupiedUntilEndOfBuffer);
        }

        _size -= actualSize;
        _head = (_head + actualSize) % _buffer.Length;
        return (arr, actualSize);
    }

    public T[] UnloadAllData()
    {
        if (_size == 0)
        {
            return [];
        }

        var arr = new T[_size];
        if (_head < _tail)
        {
            Array.Copy(_buffer, _head, arr, 0, _size);
        }
        else
        {
            var spaceOccupiedUntilEndOfBuffer = _buffer.Length - _head;
            Array.Copy(_buffer, _head, arr, 0, spaceOccupiedUntilEndOfBuffer);
            Array.Copy(_buffer, 0, arr, spaceOccupiedUntilEndOfBuffer, _tail);
        }

        _tail = _head = _size = 0;
        return arr;
    }

    public bool HasSpace(int space)
    {
        return space <= _buffer.Length - _size;
    }
}
