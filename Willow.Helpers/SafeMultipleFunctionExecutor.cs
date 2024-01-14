using System.Collections.Concurrent;

namespace Willow.Helpers;

public static class SafeMultipleFunctionExecutor
{
    /// <summary>
    /// This is a helper that runs a function over any input, safely capturing exceptions and returning them.
    /// </summary>
    /// <param name="inputs">The input over each a function is run.</param>
    /// <param name="input2">A parameter to pass along with each input.</param>
    /// <param name="func">The function to run over the inputs.</param>
    /// <param name="beforeProcessingStarted">A function that is ran before <paramref name="func"/> is ran.</param>
    /// <param name="onException">A function that runs with every exception.</param>
    /// <param name="afterProcessing">A function that runs after <paramref name="func"/> is ran.</param>
    /// <typeparam name="T1">The type of input, inputs is bound to.</typeparam>
    /// <typeparam name="T2">The type of the additional parameter.</typeparam>
    /// <remarks>
    /// NOTE THAT THE API SURFACE IN THE HELPERS MODULE IS NOT STABLE AND BREAKING CHANGES MIGHT BE APPLIED TO
    /// IT WITHOUT NOTICE!
    /// </remarks>
    /// <returns>A collection of all caught exceptions</returns>
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

    /// <inheritdoc cref="Execute{T1,T2}"/>
    /// <remarks>
    /// All of the functions are ran in a parallel manner, and as such considerations should be made to keep them pure.
    /// </remarks>
    public static async Task<Exception[]> ExecuteAsync<T1, T2>(IEnumerable<T1> inputs,
                                                               T2 input2,
                                                               Func<T1, T2, Task> func,
                                                               Action<T1, T2>? beforeProcessingStarted = null,
                                                               Action<T1, T2, Exception>? onException = null,
                                                               Action<T1, T2>? afterProcessing = null)
    {
        ConcurrentBag<Exception> exceptions = [];
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
                exceptions.Add(ex);
            }

            afterProcessing?.Invoke(input, input2);
        });

        await Task.WhenAll(tasks);
        return exceptions.ToArray();
    }
}