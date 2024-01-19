using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

using TypeExtensions = Willow.Helpers.Extensions.TypeExtensions;

namespace Tests.Helpers;

internal sealed class TestLogger<T> : ILogger<T>
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestLogger(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public void Log<TState>(LogLevel logLevel,
                            EventId eventId,
                            TState state,
                            Exception? exception,
                            Func<TState, Exception?, string> formatter)
    {
        _testOutputHelper.WriteLine(
            $"[{logLevel} - {DateTime.Now:s} - {TypeExtensions.GetFullName<T>()}:{eventId}]: {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return new Scope();
    }

    private sealed record Scope : IDisposable
    {
        public void Dispose()
        {
            //Empty
        }
    }
}
