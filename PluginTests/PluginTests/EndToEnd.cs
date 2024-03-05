using System.Reflection;

using DryIoc.Microsoft.DependencyInjection;

using Tests.Helpers;

using Willow.Registration;

using Xunit.Abstractions;

namespace PluginTests;

public sealed class EndToEnd : IDisposable
{
    private readonly IPluginLoader _sut;
    private readonly IServiceProvider _provider;
    private const string PluginAName = "PluginA";
    private const string PluginBName = "PluginB";

    public EndToEnd(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection();
        services.AddTestLogger(outputHelper);
        services.AddSettings();
        services.AddRegistration();
        services.AddSingleton<IAssemblyRegistrar, TestAssemblyRegistrar>();
        services.AddSingleton<TestAssemblyRegistrar>(static provider =>
                                                         provider.GetServices<IAssemblyRegistrar>()
                                                                 .OfType<TestAssemblyRegistrar>()
                                                                 .First());
        _provider = services.CreateServiceProvider();
        _sut = _provider.GetRequiredService<IPluginLoader>();
    }

    [Fact]
    public async Task When_AddingPlugin_CanCallCoreLibraries()
    {
        await _sut.LoadPluginAsync(PluginAName);
        _provider.GetServices<IAssemblyRegistrar>()
                 .Select(static x => x.GetType().Name)
                 .Should()
                 .Contain(static x => x == "PluginAAssemblyRegistrar");
    }

    [Fact]
    public async Task When_AddingSecondPlugin_DefinedExtensionTypesAreTheSameAsInTheSecondPlugin()
    {
        await _sut.LoadPluginAsync(PluginAName);
        await _sut.LoadPluginAsync(PluginBName);

        var testRegistrar = _provider.GetRequiredService<TestAssemblyRegistrar>();
        testRegistrar.PluginA.TryGetTarget(out var pluginA);
        var extensionType = pluginA!.DefinedTypes.First(static type => type.Name == "ITestExtensionType").AsType();

        _provider.GetServices(extensionType)
                 .Where(static x => x is not null)
                 .Select(static x => x!.GetType().Name)
                 .Should()
                 .Contain(static x => x == "PluginBExtensionType");
    }

    [Fact]
    public async Task When_LoadingPluginWithNewDependencies_NoExceptions()
    {
        await this.Invoking(async _ => await _sut.LoadPluginAsync(PluginBName)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task When_ReloadingPlugin_NoExceptions()
    {
        await this.Invoking(async _ => await _sut.LoadPluginAsync(PluginAName)).Should().NotThrowAsync();
        await this.Invoking(async _ => await _sut.LoadPluginAsync(PluginBName)).Should().NotThrowAsync();
        await this.Invoking(async _ => await _sut.UnloadPluginAsync(PluginBName)).Should().NotThrowAsync();
        await this.Invoking(async _ => await _sut.UnloadPluginAsync(PluginAName)).Should().NotThrowAsync();
        await this.Invoking(async _ => await _sut.LoadPluginAsync(PluginAName)).Should().NotThrowAsync();
        await this.Invoking(async _ => await _sut.LoadPluginAsync(PluginBName)).Should().NotThrowAsync();
    }

    // //This fails at the moment - further details in docs
    // [Fact]
    // public async Task When_UnloadingPlugin_WeakReferencesAreNoLongerHeld()
    // {
    //     await _sut.LoadPluginAsync(PluginAName);
    //     await _sut.LoadPluginAsync(PluginBName);
    //
    //     var testRegistrar = _provider.GetRequiredService<TestAssemblyRegistrar>();
    //     testRegistrar.PluginB.TryGetTarget(out _).Should().BeTrue();
    //
    //     await _sut.UnloadPluginAsync(PluginBName);
    //
    //     for (var i = 0; i < 10; i++)
    //     {
    //         GC.Collect(2, GCCollectionMode.Forced, true);
    //         GC.Collect();
    //         GC.WaitForPendingFinalizers();
    //         await Task.Delay(100);
    //     }
    //
    //     testRegistrar.PluginB.TryGetTarget(out _).Should().BeFalse();
    // }

    public sealed class TestAssemblyRegistrar : IAssemblyRegistrar
    {
        public WeakReference<Assembly> PluginA { get; private set; } = null!;
        public WeakReference<Assembly> PluginB { get; private set; } = null!;

        public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
        {
            switch (assembly.GetName().Name)
            {
                case "PluginA":
                    PluginA = new WeakReference<Assembly>(assembly);
                    break;
                case "PluginB":
                    PluginB = new WeakReference<Assembly>(assembly);
                    break;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        (_provider as IDisposable)?.Dispose();
    }
}
