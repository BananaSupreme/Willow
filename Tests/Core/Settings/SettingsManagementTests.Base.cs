using System.Text.Json;

using Tests.Helpers;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Eventing.Registration;
using Willow.Core.Settings.Abstractions;
using Willow.Core.Settings.Events;
using Willow.Core.Settings.Registration;

using Xunit.Abstractions;

namespace Tests.Core.Settings;

public sealed partial class SettingsManagementTests : IDisposable
{
    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    private readonly Guid _defaultGuid = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
    private readonly Guid _newGuid = Guid.Parse("D3E4C79A-41D0-45A6-B0E7-AE162F7BFB48");
    private readonly ServiceProvider _provider;

    public SettingsManagementTests(ITestOutputHelper testOutputHelper)
    {
        var services = new ServiceCollection();
        services.AddTestLogger(testOutputHelper);
        new EventingRegistrar().RegisterServices(services);
        new SettingsRegistrar().RegisterServices(services);
        services.AddSingleton<TestHandler<TestSettings>>();
        services.AddSingleton<TestHandler<TestSettingsRecord>>();
        services.AddSingleton<TestHandler<TestSettingsRecordStruct>>();
        _provider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _provider.Dispose();
        File.Delete(ISettings<TestSettings>.SettingsFilePath);
        File.Delete(ISettings<TestSettingsRecord>.SettingsFilePath);
        File.Delete(ISettings<TestSettingsRecordStruct>.SettingsFilePath);
    }

    private void When_RequestingSettingsForTheFirstTime_RegisteredWithDefaultValues<T>() where T : ITestSettings, new()
    {
        var settings = _provider.GetRequiredService<ISettings<T>>();
        settings.CurrentValue.Id.Should().Be(_defaultGuid);
    }

    private async Task When_RequestingSettingsWithExistingFile_UseExistingSettings<T>() where T : ITestSettings, new()
    {
        Directory.CreateDirectory(ISettings<T>.SettingsFolderPath);
        await using (var file = File.CreateText(ISettings<T>.SettingsFilePath))
        {
            await file.WriteAsync(JsonSerializer.Serialize(new T { Id = _newGuid }, _serializerOptions));
            await file.FlushAsync();
        }

        var settings = _provider.GetRequiredService<ISettings<T>>();
        settings.Flush();
        settings.CurrentValue.Id.Should().Be(_newGuid);
    }

    private void When_UpdateCalled_ValueShouldBeUpdated<T>() where T : ITestSettings, new()
    {
        var settings = _provider.GetRequiredService<ISettings<T>>();
        settings.Update(new T { Id = _newGuid });
        settings.Flush();
        settings.CurrentValue.Id.Should().Be(_newGuid);
    }

    private async Task When_UpdateCalled_FileUpdated<T>() where T : ITestSettings, new()
    {
        var settings = _provider.GetRequiredService<ISettings<T>>();
        settings.Update(new T { Id = _newGuid });
        settings.Update(new T { Id = _defaultGuid });
        settings.Update(new T { Id = _newGuid });
        settings.Flush();

        ((IDisposable)settings).Dispose();

        await using var file = File.OpenRead(GetSettingsFilePath<T>());
        var settingsFromFile = await JsonSerializer.DeserializeAsync<T>(file, _serializerOptions);
        settingsFromFile.Should().BeEquivalentTo(new T { Id = _newGuid });
    }

    private void When_UpdateCalled_SettingsChangedEventShouldBeTriggered<T>() where T : ITestSettings, new()
    {
        var dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        dispatcher.RegisterHandler<SettingsUpdatedEvent<T>, TestHandler<T>>();
        var settings = _provider.GetRequiredService<ISettings<T>>();
        var oldValue = settings.CurrentValue;

        settings.Update(new T { Id = _newGuid });
        settings.Flush();
        dispatcher.Flush();

        var handler = _provider.GetRequiredService<TestHandler<T>>();
        handler.Event.HasValue.Should().BeTrue();
        handler.Event!.Value.OldValue.Should().BeEquivalentTo(oldValue);
        handler.Event!.Value.NewValue.Should().BeEquivalentTo(settings.CurrentValue);
    }

    private void When_UpdateCalledWithSameValue_SettingsChangedEventShouldNotBeTriggered<T>()
        where T : ITestSettings, new()
    {
        var dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        dispatcher.RegisterHandler<SettingsUpdatedEvent<T>, TestHandler<T>>();
        var settings = _provider.GetRequiredService<ISettings<T>>();

        settings.Update(new T { Id = _defaultGuid });
        settings.Flush();
        dispatcher.Flush();

        var handler = _provider.GetRequiredService<TestHandler<T>>();
        handler.Event.HasValue.Should().BeFalse();
    }

    private void When_FileUpdateAttempt_IOException<T>() where T : ITestSettings, new()
    {
        var settings = _provider.GetRequiredService<ISettings<T>>();
        settings.Flush();
        this.Invoking(static _ =>
            {
                using var __ = File.OpenWrite(ISettings<T>.SettingsFilePath);
            })
            .Should()
            .Throw<IOException>();
    }

    private async Task When_CorruptedFileInRegistration_DefaultValuesShouldBeUsedAndFileCorrected<T>()
        where T : ITestSettings, new()
    {
        await using (var file = File.Create(GetSettingsFilePath<T>()))
        {
            await file.WriteAsync("a"u8.ToArray().AsMemory());
            await file.FlushAsync();
        }

        var originalSettings = _provider.GetRequiredService<ISettings<T>>();
        originalSettings.Flush();

        originalSettings.CurrentValue.Id.Should().Be(_defaultGuid);
        ((IDisposable)originalSettings).Dispose();
        await using (var file = File.OpenRead(GetSettingsFilePath<T>()))
        {
            var settingsFromFile = await JsonSerializer.DeserializeAsync<T>(file, _serializerOptions);
            settingsFromFile.Should().BeEquivalentTo(new T { Id = _defaultGuid });
        }
    }

    private static string GetSettingsFilePath<T>() where T : new()
    {
        Directory.CreateDirectory(ISettings<T>.SettingsFolderPath);

        return ISettings<T>.SettingsFilePath;
    }

    public interface ITestSettings
    {
        Guid Id { get; init; }
    }

    private sealed class TestHandler<T> : IEventHandler<SettingsUpdatedEvent<T>>
    {
        public SettingsUpdatedEvent<T>? Event { get; private set; }

        public Task HandleAsync(SettingsUpdatedEvent<T> @event)
        {
            Event = @event;
            return Task.CompletedTask;
        }
    }
}
