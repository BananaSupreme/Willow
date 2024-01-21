using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Tests.Helpers;

internal sealed class TestLoggerFactory : ILoggerFactory
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestLoggerFactory(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger<TestLoggerFactory>(_testOutputHelper);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        //empty
    }

    public void Dispose()
    {
        //empty
    }
}
