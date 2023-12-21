using System.Collections;

namespace Willow.WhisperServer.Helpers;

public readonly struct PyIterEnumerable : IEnumerable<PyObject>
{
    private readonly PyIter _pyIter1;

    public PyIterEnumerable(PyIter pyIter)
    {
        _pyIter1 = pyIter;
    }

    public IEnumerator<PyObject> GetEnumerator()
    {
        return _pyIter1;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _pyIter1;
    }
}