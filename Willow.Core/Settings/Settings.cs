using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Settings.Events;
using Willow.Helpers.Locking;

namespace Willow.Core.Settings;

internal sealed class Settings<T> : ISettings<T>, IDisposable, IAsyncDisposable
    where T : new()
{
    private readonly DisposableLock _lock = new();
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };
    private FileStream _file;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IQueuedFileWriter _writer;
    private readonly ILogger<Settings<T>> _log;

    public T CurrentValue { get; private set; }

    public Settings(IEventDispatcher eventDispatcher, 
                    IQueuedFileWriter writer,
                    ILogger<Settings<T>> log)
    {
        _eventDispatcher = eventDispatcher;
        _writer = writer;
        _log = log;

        Initialize();
    }

    public void Update(T newValue)
    {
        using var _ = _lock.Lock();
        var oldJson = GetValueAsJson();
        var newJson = JsonSerializer.Serialize(newValue, _options);
        if (oldJson == newJson)
        {
            _log.ValueUpdatedWithOldValue();
            return;
        }

        _log.UpdatingValue(oldJson, newJson);
        var oldValue = CurrentValue;
        CurrentValue = newValue;
        _writer.QueueRequest(new(_file, newJson));
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
            _log.CreatingNewFile(ISettings<T>.SettingsFilePath);
            _file = File.Open(ISettings<T>.SettingsFilePath, FileMode.Create, FileAccess.ReadWrite);
            SetDefault();
        }
        else
        {
            _log.FoundExistingFile(ISettings<T>.SettingsFilePath);
            _file = File.Open(ISettings<T>.SettingsFilePath, FileMode.Open, FileAccess.ReadWrite);
            ReadFromFileAsync().GetAwaiter().GetResult();
        }
    }

    [MemberNotNull(nameof(CurrentValue))]
    private void SetDefault()
    {
        CurrentValue = new();
        _log.SetDefault(GetValueAsJson());
        _writer.QueueRequest(new(_file, GetValueAsJson()));
    }
    
    [MemberNotNull(nameof(CurrentValue))]
    private async Task ReadFromFileAsync()
    {
        try
        {
            CurrentValue = await JsonSerializer.DeserializeAsync<T>(_file, _options) ?? throw new FileLoadException();
            _log.ValueReadFromFile(GetValueAsJson());
        }
        catch (Exception ex)
        {
            _log.ValueReadingFailed(ISettings<T>.SettingsFilePath, ex);
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

internal static partial class SettingsLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Requested update function was called with the old value, aborting.")]
    public static partial void ValueUpdatedWithOldValue(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Updating value of settings\r\nold Value: {oldValue}\r\nnew Value: {newValue}")]
    public static partial void UpdatingValue(this ILogger logger, string oldValue, string newValue);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Creating new settings file at path ({path})")]
    public static partial void CreatingNewFile(this ILogger logger, string path);
    
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "A file was found already for the settings at path ({path})")]
    public static partial void FoundExistingFile(this ILogger logger, string path);
    
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Setting default value of settings ({value})")]
    public static partial void SetDefault(this ILogger logger, string value);
    
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "Value ({value}) was read from file")]
    public static partial void ValueReadFromFile(this ILogger logger, string value);
    
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "Tried reading file at path ({path}) and failed, reverting to default")]
    public static partial void ValueReadingFailed(this ILogger logger, string path, Exception ex);
}