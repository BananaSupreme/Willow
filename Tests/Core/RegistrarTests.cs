using System.Reflection;
using System.Reflection.Emit;

using DryIoc.Microsoft.DependencyInjection;

using Tests.Helpers;

using Willow.Core.Registration;
using Willow.Core.Registration.Abstractions;
using Willow.Core.Registration.Exceptions;

using Xunit.Abstractions;

namespace Tests.Core;

public sealed class RegistrarTests : IDisposable
{
    private readonly IAssemblyRegistrationEntry _sut;
    private readonly AssemblyRegistrarTestDouble _registrar;
    private readonly IServiceProvider _provider;

    public RegistrarTests(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection();
        services.AddTestLogger(outputHelper);
        services.AddSingleton(typeof(ICollectionProvider<>), typeof(CollectionProvider<>));
        services.AddSingleton<IAssemblyRegistrar, AssemblyRegistrarTestDouble>();
        services.AddSingleton<IAssemblyRegistrationEntry, AssemblyRegistrationEntry>();
        _provider = services.CreateServiceProvider();
        _registrar = (AssemblyRegistrarTestDouble)_provider.GetRequiredService<IAssemblyRegistrar>();
        _sut = _provider.GetRequiredService<IAssemblyRegistrationEntry>();
    }

    [Fact]
    public async Task When_AssemblyRegistersService_IsCallable()
    {
        _registrar.RegisterAction = static s => s.AddSingleton<TestClass>();
        await RunRegistration();
        _provider.GetService<TestClass>().Should().NotBeNull();
    }

    [Fact]
    public async Task When_RegisterNoneSingletonServices_Exception()
    {
        _registrar.RegisterAction = static s => s.AddTransient<TestClass>();
        await this.Invoking(async _ => await RunRegistration())
                  .Should()
                  .ThrowAsync<AssemblyLoadingException>()
                  .WithInnerException(typeof(RegistrationMustBeSingletonException));
    }

    [Fact]
    public async Task When_RegistersServiceWithMultipleInterfaces_ItIsReachableByAllAndTheSameServiceIsReturned()
    {
        _registrar.RegisterAction = static s =>
        {
            s.AddSingleton<ITestInterface1, TestClass>();
            s.AddSingleton<ITestInterface2, TestClass>();
        };
        await RunRegistration();
        var out1 = _provider.GetRequiredService<ITestInterface1>();
        var out2 = _provider.GetRequiredService<ITestInterface2>();
        out1.Should().BeSameAs(out2);
    }

    [Fact]
    public async Task When_RegistersService_ReachableByConcreteAndInterfaceType()
    {
        _registrar.RegisterAction = static s => s.AddSingleton<ITestInterface1, TestClass>();
        await RunRegistration();
        var out1 = _provider.GetRequiredService<ITestInterface1>();
        var out2 = _provider.GetRequiredService<TestClass>();
        out1.Should().BeSameAs(out2);
    }

    [Fact]
    public async Task When_RegistersServiceAfterConcrete_ReachableByConcreteAndInterfaceType()
    {
        _registrar.RegisterAction = static s => s.AddSingleton<TestClass>();
        await RunRegistration();
        _registrar.RegisterAction = static s => s.AddSingleton<ITestInterface1, TestClass>();
        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("test"), AssemblyBuilderAccess.Run);
        await _sut.RegisterAssemblyAsync(dynamicAssembly);
        var out1 = _provider.GetRequiredService<ITestInterface1>();
        var out2 = _provider.GetRequiredService<TestClass>();
        out1.Should().BeSameAs(out2);
    }

    [Fact]
    public async Task When_AssemblyUnregisters_ServiceNoLongerCallable()
    {
        _registrar.RegisterAction = static s => s.AddSingleton<TestClass>();
        var guid = await RunRegistration();
        _provider.GetService<TestClass>().Should().NotBeNull();
        await _sut.UnregisterAssemblyAsync(guid);
        _provider.GetService<TestClass>().Should().BeNull();
    }

    [Fact]
    public async Task When_AssemblyUnregistersInterface_ServiceNoLongerCallable()
    {
        _registrar.RegisterAction = static s => s.AddSingleton<ITestInterface1, TestClass>();
        var guid = await RunRegistration();
        _provider.GetService<TestClass>().Should().NotBeNull();
        _provider.GetService<ITestInterface1>().Should().NotBeNull();
        await _sut.UnregisterAssemblyAsync(guid);
        _provider.GetService<TestClass>().Should().BeNull();
        _provider.GetService<ITestInterface1>().Should().BeNull();
    }


    [Fact]
    public async Task When_AssemblyUnregisters_CollectionServicesLoseOnlyAssemblyDefinedServices()
    {
        _registrar.RegisterAction = static s => s.AddSingleton<ITestInterface1, TestClass>();
        await RunRegistration();
        _registrar.RegisterAction = static s => s.AddSingleton<ITestInterface1, TestClass2>();
        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("test"), AssemblyBuilderAccess.Run);
        var guid = await _sut.RegisterAssemblyAsync(dynamicAssembly);
        _provider.GetServices<ITestInterface1>().Should().HaveCount(2);
        await _sut.UnregisterAssemblyAsync(guid);
        _provider.GetServices<ITestInterface1>().Should().ContainSingle();
        _provider.GetServices<ITestInterface1>().First().Should().BeOfType<TestClass>();
    }
    
    [Fact]
    public async Task When_AssemblyUnregisters_CollectionProviderUpdates()
    {
        _registrar.RegisterAction = static s => s.AddSingleton<ITestInterface1, TestClass>();
        await RunRegistration();
        _registrar.RegisterAction = static s => s.AddSingleton<ITestInterface1, TestClass2>();
        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("test"), AssemblyBuilderAccess.Run);
        var guid = await _sut.RegisterAssemblyAsync(dynamicAssembly);
        var provider = _provider.GetRequiredService<ICollectionProvider<ITestInterface1>>();
        provider.Get().Should().HaveCount(2);
        await _sut.UnregisterAssemblyAsync(guid);
        provider.Get().Should().ContainSingle();
        provider.Get().First().Should().BeOfType<TestClass>();
    }

    [Fact]
    public async Task When_AssemblyUnregisters_ServiceDisposed()
    {
        _registrar.RegisterAction = static s =>
        {
            s.AddSingleton<TestClassDisposable>();
            s.AddSingleton<TestClassAsyncDisposable>();
            s.AddSingleton<TestClassAllDisposable>();
        };

        var guid = await RunRegistration();
        var item1 = _provider.GetRequiredService<TestClassDisposable>();
        var item2 = _provider.GetRequiredService<TestClassAsyncDisposable>();
        var item3 = _provider.GetRequiredService<TestClassAllDisposable>();
        await _sut.UnregisterAssemblyAsync(guid);
        item1.Disposed.Should().BeTrue();
        item2.Disposed.Should().BeTrue();
        item3.DisposedCount.Should().Be(1);
    }

    [Fact]
    public async Task When_AssemblyUnregistersAndRegisters_ServiceReturnsFresh()
    {
        _registrar.RegisterAction = static s => s.AddSingleton<TestClassDisposable>();
        var guid = await RunRegistration();
        _provider.GetRequiredService<TestClassDisposable>().Should().NotBeNull();
        await _sut.UnregisterAssemblyAsync(guid);
        _provider.GetService<TestClassDisposable>().Should().BeNull();
        _ = await RunRegistration();
        _provider.GetRequiredService<TestClassDisposable>().Disposed.Should().BeFalse();
    }

    [Fact]
    public async Task When_ServiceRegistrationFails_NoServicesLoaded()
    {
        _registrar.RegisterAction = static s =>
        {
            s.AddSingleton<TestClass>();
            throw new Exception();
        };
        await this.Invoking(async _ => await RunRegistration()).Should().ThrowAsync<AssemblyLoadingException>();
        _provider.GetService<TestClass>().Should().BeNull();
    }

    [Fact]
    public async Task When_LoadingAssemblyTwice_Exception()
    {
        await RunRegistration();
        await this.Invoking(async _ => await RunRegistration())
                  .Should()
                  .ThrowAsync<AssemblyLoadingException>()
                  .WithInnerException(typeof(AssemblyAlreadyLoadedException));
    }

    [Fact]
    public async Task When_UnloadingNonExistentAssembly_Exception()
    {
        await this.Invoking(async _ => await _sut.UnregisterAssemblyAsync(Guid.NewGuid()))
                  .Should()
                  .ThrowAsync<AssemblyNotFoundException>();
    }

    [Fact]
    public async Task When_UnloadingNonExistentAssemblyThatWasExistent_Exception()
    {
        var guid = await RunRegistration();
        await _sut.UnregisterAssemblyAsync(guid);
        await this.Invoking(async _ => await _sut.UnregisterAssemblyAsync(guid))
                  .Should()
                  .ThrowAsync<AssemblyNotFoundException>();
    }

    [Fact]
    public async Task When_StartIsCalled_ServicesFromOtherRegistrationCyclesAreNotAvailable()
    {
        IServiceProvider innerProvider = null!;
        _registrar.RegisterAction = static s => { s.AddSingleton<TestClass>(); };
        _registrar.StartAction = provider => innerProvider = provider;
        await RunRegistration();
        innerProvider.GetService<TestClass>().Should().NotBeNull();
        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("test"), AssemblyBuilderAccess.Run);

        _registrar.RegisterAction = null;
        await _sut.RegisterAssemblyAsync(dynamicAssembly);
        innerProvider.GetService<TestClass>().Should().BeNull();
        _provider.GetService<TestClass>().Should().NotBeNull();
    }

    [Fact]
    public async Task When_StartRequestsAllTypesOfCertainType_UnableToReachTypesOutsideItsOwnAssembly()
    {
        IServiceProvider innerProvider = null!;
        _registrar.RegisterAction = static s => { s.AddSingleton<ITestInterface1, TestClass>(); };
        _registrar.StartAction = provider => innerProvider = provider;
        await RunRegistration();
        innerProvider.GetService<TestClass>().Should().NotBeNull();

        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("test"), AssemblyBuilderAccess.Run);
        var moduleBuilder = dynamicAssembly.DefineDynamicModule("test");
        var typeBuilder = moduleBuilder.DefineType("TestClass2");
        typeBuilder.AddInterfaceImplementation(typeof(ITestInterface1));
        var newType = typeBuilder.CreateType();

        _registrar.RegisterAction = s => { s.AddSingleton(typeof(ITestInterface1), newType); };
        await _sut.RegisterAssemblyAsync(dynamicAssembly);
        _provider.GetServices<ITestInterface1>().Should().HaveCount(2);
        innerProvider.GetServices<ITestInterface1>().Should().ContainSingle();
        innerProvider.GetServices<ITestInterface1>().First().Should().BeOfType(newType);
    }

    [Fact]
    public async Task When_StartFunctionFails_ServicesAreRemoved()
    {
        _registrar.RegisterAction = static s => { s.AddSingleton<TestClass>(); };
        _registrar.StartAction = static _ => throw new Exception();
        await this.Invoking(async _ => await RunRegistration()).Should().ThrowAsync<AssemblyLoadingException>();

        _provider.GetService<TestClass>().Should().BeNull();
    }

    [Fact]
    public async Task When_StartFunctionFails_StopIsCalled()
    {
        var called = false;
        _registrar.StartAction = static _ => throw new Exception();
        _registrar.StopAction = _ => called = true;
        await this.Invoking(async _ => await RunRegistration()).Should().ThrowAsync<AssemblyLoadingException>();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task When_StartAndStopFails_ServicesStillRemoved()
    {
        _registrar.RegisterAction = static s => { s.AddSingleton<TestClass>(); };
        _registrar.StartAction = static _ => throw new Exception();
        _registrar.StopAction = static _ => throw new Exception();
        await this.Invoking(async _ => await RunRegistration()).Should().ThrowAsync<AssemblyLoadingException>();

        _provider.GetService<TestClass>().Should().BeNull();
    }

    [Fact]
    public async Task When_StartAndStopLongRunning_CreationNotBlocked()
    {
        _registrar.StartAction = static _ => Task.Delay(10000);
        _registrar.StopAction = static _ => Task.Delay(10000);
        await this.Invoking(async _ => await RunRegistration().WaitAsync(TimeSpan.FromSeconds(5)))
                  .Should()
                  .NotThrowAsync();
    }

    [Fact]
    public async Task When_AssemblyDefinesIAssemblyRegistrar_ShouldLoadAsWell()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(typeof(ICollectionProvider<>), typeof(CollectionProvider<>));
        services.AddSingleton<IAssemblyRegistrationEntry, AssemblyRegistrationEntry>();
        var provider = services.CreateServiceProvider();
        var sut = provider.GetRequiredService<IAssemblyRegistrationEntry>();
        await sut.RegisterAssemblyAsync(GetType().Assembly);
        provider.GetService<CountingAssemblyRegistrarTestDouble>().Should().NotBeNull();
        provider.GetService<CountingAssemblyRegistrarTestDouble>()!.Called.Should().BeTrue();
    }

    private async Task<Guid> RunRegistration()
    {
        return await _sut.RegisterAssemblyAsync(GetType().Assembly);
    }

    public interface ITestInterface1;

    private interface ITestInterface2;

    private sealed class TestClass : ITestInterface1, ITestInterface2;

    private sealed class TestClass2 : ITestInterface1, ITestInterface2;

    private sealed class TestClassDisposable : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    private sealed class TestClassAsyncDisposable : IAsyncDisposable
    {
        public bool Disposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TestClassAllDisposable : IDisposable, IAsyncDisposable
    {
        public int DisposedCount { get; private set; }

        public void Dispose()
        {
            DisposedCount++;
        }

        public ValueTask DisposeAsync()
        {
            DisposedCount++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class AssemblyRegistrarTestDouble : IAssemblyRegistrar
    {
        public Action<IServiceCollection>? RegisterAction { get; set; }
        public Action<IServiceProvider>? StartAction { get; set; }
        public Action<IServiceProvider>? StopAction { get; set; }

        public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
        {
            RegisterAction?.Invoke(services);
        }

        public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
        {
            StartAction?.Invoke(serviceProvider);
            return Task.CompletedTask;
        }

        public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
        {
            StopAction?.Invoke(serviceProvider);
            return Task.CompletedTask;
        }
    }

    public sealed class CountingAssemblyRegistrarTestDouble : IAssemblyRegistrar
    {
        private int _callers;
        public bool Called => _callers >= 2;

        public void Register(Assembly assembly, Guid assemblyId, IServiceCollection services)
        {
            _callers++;
        }

        public Task StartAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
        {
            _callers++;
            return Task.CompletedTask;
        }

        public Task StopAsync(Assembly assembly, Guid assemblyId, IServiceProvider serviceProvider)
        {
            _callers++;
            return Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        (_provider as IDisposable)?.Dispose();
    }
}
