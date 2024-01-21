namespace Willow.Core.Middleware;

public delegate Task Middleware<T>(T input, Func<T, Task> next);

public delegate Task Handler<in T>(T input);
