using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Willow.Helpers.Extensions;

namespace Willow.Registration;

internal sealed class BackgroundWorkerAssemblyRegistrar : IAssemblyRegistrar, IDisposable, IAsyncDisposable
{
    private readonly Dictionary<Guid, TaskRunningInfo> _taskRunningInfos = [];

    public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
    {
        services.AddAllTypesDeriving<IBackgroundWorker>(assembly);
    }

    public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        CancellationTokenSource cts = new();
        var workers = serviceProvider.GetServices<IBackgroundWorker>();
        var tasks = workers.Select(async x => await x.StartAsync(cts.Token)).ToArray();

        EnsureNoTasksFaulted(tasks);

        _taskRunningInfos.Add(assemblyId, new TaskRunningInfo(tasks, cts));
        return Task.CompletedTask;
    }

    private static void EnsureNoTasksFaulted(Task[] tasks)
    {
        if (!tasks.Any(static x => x.IsFaulted || x.IsCanceled))
        {
            return;
        }

        var exceptions = tasks.Where(static x => x.IsFaulted).Select<Task, Exception>(static x => x.Exception!);
        var cancelledExceptions = tasks.Where(static x => x.IsCanceled)
                                       .Select<Task, Exception>(static x => new TaskCanceledException(x));
        exceptions = exceptions.Union(cancelledExceptions);
        throw new AggregateException(exceptions);
    }

    public async Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
    {
        var taskRunningInfo = _taskRunningInfos[assemblyId];
        try
        {
            await taskRunningInfo.CancellationTokenSource.CancelAsync();
        }
        finally
        {
            var tasks = taskRunningInfo.RunningTasks.Select(async static t =>
                                                                await t.ConfigureAwait(
                                                                    ConfigureAwaitOptions.SuppressThrowing));
            await tasks.WhenAll();
            var workers = serviceProvider.GetServices<IBackgroundWorker>();
            await workers.Select(static async x => await x.StopAsync()).WhenAll();
            _taskRunningInfos.Remove(assemblyId);
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await _taskRunningInfos.Select(async static x =>
        {
            try
            {
                await x.Value.CancellationTokenSource.CancelAsync()
                       .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
            finally
            {
                await Task.WhenAll(x.Value.RunningTasks);
            }
        }).WhenAll();
    }

    private readonly record struct TaskRunningInfo(Task[] RunningTasks, CancellationTokenSource CancellationTokenSource);
}
