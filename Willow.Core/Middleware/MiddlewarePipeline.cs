using Willow.Core.Middleware.Abstractions;

namespace Willow.Core.Middleware;

internal sealed class MiddlewarePipeline<T> : IMiddlewarePipeline<T>
{
    private readonly List<Middleware<T>> _middlewareSteps;

    public MiddlewarePipeline(List<Middleware<T>> middlewareSteps)
    {
        _middlewareSteps = middlewareSteps;
    }

    public async Task ExecuteAsync(T input, Handler<T> end)
    {
        if (_middlewareSteps.Count == 0)
        {
            await end(input);
        }

        var firstStep = GetStep(end, 0);
        await firstStep(input);
    }

    public Func<T, Task> GetStep(Handler<T> end, int index)
    {
        if (index >= _middlewareSteps.Count)
        {
            return input => end(input);
        }

        var nextStep = _middlewareSteps[index];
        return input => nextStep(input, GetStep(end, index + 1));
    }
}
