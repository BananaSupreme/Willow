using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Eventing.Registration;
using Willow.Core.Settings.Abstractions;
using Willow.Core.Settings.Events;
using Willow.Core.Settings.Registration;

namespace Tests.Core;

public sealed class SettingsManagementTests : IDisposable
{
    private readonly Guid _defaultGuid = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
    private readonly Guid _newGuid = Guid.Parse("D3E4C79A-41D0-45A6-B0E7-AE162F7BFB48");
    private ServiceProvider _provider;

    public SettingsManagementTests()
    {
        Setup();
    }

    [MemberNotNull(nameof(_provider))]
    private void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        EventingRegistrar.RegisterServices(services);
        SettingsRegistrar.RegisterServices(services);
        services.AddSingleton<TestHandler>();
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public void When_RequestingSettingsForTheFirstTime_RegisteredWithDefaultValues()
    {
        var settings = _provider.GetRequiredService<ISettings<TestSettings>>();
        settings.CurrentValue.Id.Should().Be(_defaultGuid);
    }

    [Fact]
    public async Task When_RequestingSettingsWithExistingFile_UseExistingSettings()
    {
        Directory.CreateDirectory(ISettings<TestSettings>.SettingsFolderPath);
        await using (var file = File.CreateText(ISettings<TestSettings>.SettingsFilePath))
        {
            await file.WriteAsync(JsonSerializer.Serialize(new TestSettings(_newGuid),
                new JsonSerializerOptions() { WriteIndented = true }));
            await file.FlushAsync();
        }

        var settings = _provider.GetRequiredService<ISettings<TestSettings>>();
        settings.Flush();
        settings.CurrentValue.Id.Should().Be(_newGuid);
    }

    [Fact]
    public void When_UpdateCalled_ValueShouldBeUpdated()
    {
        var settings = _provider.GetRequiredService<ISettings<TestSettings>>();
        settings.Update(new(_newGuid));
        settings.Flush();
        settings.CurrentValue.Id.Should().Be(_newGuid);
    }

    [Fact]
    public async Task When_UpdateCalled_FileUpdated()
    {
        var settings = _provider.GetRequiredService<ISettings<TestSettings>>();
        settings.Update(new(_newGuid));
        settings.Flush();

        ((IDisposable)settings).Dispose();
        
        await using var file = File.OpenRead(GetSettingsFilePath());
        var settingsFromFile = await JsonSerializer.DeserializeAsync<TestSettings>(file,
                                   new JsonSerializerOptions() { WriteIndented = true });
        settingsFromFile.Should().Be(new TestSettings(_newGuid));
    }

    [Fact]
    public void When_UpdateCalled_SettingsChangedEventShouldBeTriggered()
    {
        var dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        dispatcher.RegisterHandler<SettingsUpdatedEvent<TestSettings>, TestHandler>();
        var settings = _provider.GetRequiredService<ISettings<TestSettings>>();
        var oldValue = settings.CurrentValue;

        settings.Update(new(_newGuid));
        settings.Flush();

        var handler = _provider.GetRequiredService<TestHandler>();
        handler.Event.HasValue.Should().BeTrue();
        handler.Event!.Value.OldValue.Should().Be(oldValue);
        handler.Event!.Value.NewValue.Should().Be(settings.CurrentValue);
    }

    [Fact]
    public void When_FileUpdateAttempt_IOException()
    {
        var settings = _provider.GetRequiredService<ISettings<TestSettings>>();
        settings.Flush();
        this.Invoking(_ =>
            {
                using var __ = File.OpenWrite(ISettings<TestSettings>.SettingsFilePath);
            })
            .Should().Throw<IOException>();
    }

    [Fact]
    public async Task When_CorruptedFileInRegistration_DefaultValuesShouldBeUsedAndFileCorrected()
    {
        await using (var file = File.Create(GetSettingsFilePath()))
        {
            await file.WriteAsync("a"u8.ToArray().AsMemory());
            await file.FlushAsync();
        }

        var originalSettings = _provider.GetRequiredService<ISettings<TestSettings>>();
        originalSettings.Flush();
        
        originalSettings.CurrentValue.Id.Should().Be(_defaultGuid);
        ((IDisposable)originalSettings).Dispose();
        await using (var file = File.OpenRead(GetSettingsFilePath()))
        {
            var settingsFromFile = await JsonSerializer.DeserializeAsync<TestSettings>(file,
                                       new JsonSerializerOptions() { WriteIndented = true });
            settingsFromFile.Should().Be(new TestSettings(_defaultGuid));
        }
    }

    private static string GetSettingsFilePath()
    {
        Directory.CreateDirectory(ISettings<TestSettings>.SettingsFolderPath);

        return ISettings<TestSettings>.SettingsFilePath;
    }

    public void Dispose()
    {
        _provider.Dispose();
        File.Delete(ISettings<TestSettings>.SettingsFilePath);
    }

    private class TestSettings : IEquatable<TestSettings>
    {
        public bool Equals(TestSettings? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((TestSettings)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(TestSettings? left, TestSettings? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TestSettings? left, TestSettings? right)
        {
            return !Equals(left, right);
        }

        public Guid Id { get; init; }

        public TestSettings(Guid id)
        {
            Id = id;
        }

        public TestSettings()
        {
            Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
        }
    }

    private class TestHandler : IEventHandler<SettingsUpdatedEvent<TestSettings>>
    {
        public SettingsUpdatedEvent<TestSettings>? Event { get; private set; }

        public Task HandleAsync(SettingsUpdatedEvent<TestSettings> @event)
        {
            Event = @event;
            return Task.CompletedTask;
        }
    }
}