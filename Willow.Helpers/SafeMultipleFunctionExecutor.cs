namespace Willow.Helpers;

internal static class SafeMultipleFunctionExecutor
{
    public static Exception[] Execute<T1, T2>(IEnumerable<T1> inputs,
                                       T2 input2,
                                       Action<T1, T2> func,
                                       Action<T1, T2>? beforeProcessingStarted = null,
                                       Action<T1, T2, Exception>? onException = null,
                                       Action<T1, T2>? afterProcessing = null)
    {
        List<Exception> exceptions = [];
        foreach (var input in inputs)
        {
            beforeProcessingStarted?.Invoke(input, input2);
            try
            {
                func(input, input2);
            }
            catch (Exception ex)
            {
                onException?.Invoke(input, input2, ex);
                exceptions.Add(ex);
            }

            afterProcessing?.Invoke(input, input2);
        }

        return exceptions.ToArray();
    }

    public static async Task ExecuteAsync<T1, T2>(IEnumerable<T1> inputs,
                                                  T2 input2,
                                                  Func<T1, T2, Task> func,
                                                  Action<T1, T2>? beforeProcessingStarted = null,
                                                  Action<T1, T2, Exception>? onException = null,
                                                  Action<T1, T2>? afterProcessing = null)
    {
        object locker = new();
        List<Exception> exceptions = [];
        var tasks = inputs.Select(async input =>
        {
            beforeProcessingStarted?.Invoke(input, input2);
            try
            {
                await func(input, input2);
            }
            catch (Exception ex)
            {
                onException?.Invoke(input, input2, ex);
                lock (locker)
                {
                    exceptions.Add(ex);
                }
            }

            afterProcessing?.Invoke(input, input2);
        });

        await Task.WhenAll(tasks);
        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }
}