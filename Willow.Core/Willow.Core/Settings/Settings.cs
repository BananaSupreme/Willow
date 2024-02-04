using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using Willow.Eventing;
using Willow.Helpers.Locking;
using Willow.Settings.Abstractions;
using Willow.Settings.Events;
using Willow.Settings.Models;

namespace Willow.Settings;

internal sealed class Settings<T> : ISettings<T>, IDisposable, IAsyncDisposable where T : new()
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly DisposableLock _lock = new();
    private readonly ILogger<Settings<T>> _log;

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    private readonly IQueuedFileWriter _writer;
    private FileStream _file;

    public Settings(IEventDispatcher eventDispatcher, IQueuedFileWriter writer, ILogger<Settings<T>> log)
    {
        _eventDispatcher = eventDispatcher;
        _writer = writer;
        _log = log;

        Initialize();
    }

    public async ValueTask DisposeAsync()
    {
        Flush();
        await _file.DisposeAsync();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public T CurrentValue { get; private set; } = default!;

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
        _writer.QueueRequest(new FileUpdateRequest(_file, newJson));
        _eventDispatcher.Dispatch(new SettingsUpdatedEvent<T>(oldValue, newValue));
    }

    public void Flush()
    {
        _writer.Flush();
    }

    [MemberNotNull(nameof(_file))]
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
            CurrentValue = ReadFromFileAsync().GetAwaiter().GetResult();
        }
    }

    private void SetDefault()
    {
        CurrentValue = new T();
        _log.SetDefault(GetValueAsJson());
        _writer.QueueRequest(new FileUpdateRequest(_file, GetValueAsJson()));
    }

    private async Task<T> ReadFromFileAsync()
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

        return CurrentValue;
    }

    private string GetValueAsJson()
    {
        return JsonSerializer.Serialize(CurrentValue, _options);
    }
}

internal static partial class SettingsLoggingExtensions
{
    [LoggerMessage(EventId = 1,
                   Level = LogLevel.Debug,
                   Message = "Requested update function was called with the old value, aborting.")]
    public static partial void ValueUpdatedWithOldValue(this ILogger logger);

    [LoggerMessage(EventId = 2,
                   Level = LogLevel.Information,
                   Message = "Updating value of settings\r\nold Value: {oldValue}\r\nnew Value: {newValue}")]
    public static partial void UpdatingValue(this ILogger logger, string oldValue, string newValue);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Creating new settings file at path ({path})")]
    public static partial void CreatingNewFile(this ILogger logger, string path);

    [LoggerMessage(EventId = 4,
                   Level = LogLevel.Information,
                   Message = "A file was found already for the settings at path ({path})")]
    public static partial void FoundExistingFile(this ILogger logger, string path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Setting default value of settings ({value})")]
    public static partial void SetDefault(this ILogger logger, string value);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Value ({value}) was read from file")]
    public static partial void ValueReadFromFile(this ILogger logger, string value);

    [LoggerMessage(EventId = 7,
                   Level = LogLevel.Error,
                   Message = "Tried reading file at path ({path}) and failed, reverting to default")]
    public static partial void ValueReadingFailed(this ILogger logger, string path, Exception ex);
}
