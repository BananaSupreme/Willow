using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Settings.Events;

namespace Willow.Core.Settings;

internal sealed class Settings<T> : ISettings<T>, IDisposable, IAsyncDisposable
    where T : new()
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };
    private FileStream _file;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IQueuedFileWriter _writer;

    public T CurrentValue { get; private set; }

    public Settings(IEventDispatcher eventDispatcher, IQueuedFileWriter writer)
    {
        _eventDispatcher = eventDispatcher;
        _writer = writer;

        Initialize();
    }

    public void Update(T newValue)
    {
        var oldValue = CurrentValue;
        CurrentValue = newValue;
        _writer.QueueRequest(new(_file, GetValueAsJson()));
        _eventDispatcher.Dispatch(new SettingsUpdatedEvent<T>(oldValue, newValue));
    }

    public void Flush()
    {
        _writer.Flush();
    }

    [MemberNotNull(nameof(CurrentValue), nameof(_file))]
    private void Initialize()
    {
        Directory.CreateDirectory(ISettings<T>.SettingsFolderPath);

        if (!File.Exists(ISettings<T>.SettingsFilePath))
        {
            _file = File.Open(ISettings<T>.SettingsFilePath, FileMode.Create, FileAccess.ReadWrite);
            SetDefault();
        }
        else
        {
            _file = File.Open(ISettings<T>.SettingsFilePath, FileMode.Open, FileAccess.ReadWrite);
            ReadFromFileAsync().GetAwaiter().GetResult();
        }
    }

    [MemberNotNull(nameof(CurrentValue))]
    private void SetDefault()
    {
        CurrentValue = new();
        _writer.QueueRequest(new(_file, GetValueAsJson()));
    }
    
    [MemberNotNull(nameof(CurrentValue))]
    private async Task ReadFromFileAsync()
    {
        try
        {
            CurrentValue = await JsonSerializer.DeserializeAsync<T>(_file, _options) ?? throw new FileLoadException();
        }
        catch (Exception _)
        {
            SetDefault();
        }
    }

    private string GetValueAsJson()
    {
        return JsonSerializer.Serialize(CurrentValue, _options);
    }
    
    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        Flush();
        await _file.DisposeAsync();
    }
}