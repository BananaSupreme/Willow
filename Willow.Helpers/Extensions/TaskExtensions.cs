namespace Willow.Helpers.Extensions;

public static class TaskExtensions
{
    public static Task WhenAll(this IEnumerable<Task> tasks)
    {
        var tasksArray = tasks.ToArray();
        try
        {
            return Task.WhenAll(tasksArray);
        }
        catch (Exception)
        {
            var exceptions = tasksArray.Where(static x => x.Exception is not null)
                                       .Select(static x => (Exception)x.Exception!);
            throw new AggregateException(exceptions);
        }
    }

    public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
    {
        var tasksArray = tasks.ToArray();
        try
        {
            return Task.WhenAll(tasksArray);
        }
        catch (Exception)
        {
            var exceptions = tasksArray.Where(static x => x.Exception is not null)
                                       .Select(static x => (Exception)x.Exception!);
            throw new AggregateException(exceptions);
        }
    }
}
