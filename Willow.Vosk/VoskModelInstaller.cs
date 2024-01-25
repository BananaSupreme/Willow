using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;

using Microsoft.Extensions.Logging;

using Willow.Core.Settings.Abstractions;
using Willow.Helpers.Extensions;
using Willow.Helpers.Locking;
using Willow.Helpers.Logging.Extensions;
using Willow.Helpers.Logging.Loggers;
using Willow.Vosk.Abstractions;
using Willow.Vosk.Enums;
using Willow.Vosk.Settings;

namespace Willow.Vosk;

internal sealed class VoskModelInstaller : IVoskModelInstaller
{
    private readonly IVoskModelDownloader _downloader;
    private readonly DisposableLock _lock = new();
    private readonly ILogger<VoskModelInstaller> _log;
    private readonly ISettings<VoskSettings> _voskSettings;

    public VoskModelInstaller(ISettings<VoskSettings> voskSettings,
                              IVoskModelDownloader downloader,
                              ILogger<VoskModelInstaller> log)
    {
        _voskSettings = voskSettings;
        _downloader = downloader;
        _log = log;
    }

    public async Task EnsureExistsAsync()
    {
        using var _ = await _lock.LockAsync();
        _log.AddContext("modelSettings", _voskSettings.CurrentValue);
        _log.CheckingModelFilesState();
        if (await ValidateModelFilesAsync(_voskSettings.CurrentValue))
        {
            _log.FilesAlreadyOk();
            return;
        }

        _log.DownloadingModel();
        await DownloadModelFileAsync(_voskSettings.CurrentValue.ModelPath, _voskSettings.CurrentValue.VoskModel);
        _log.CreatingChecksum();
        await CreateChecksumFileAsync(_voskSettings.CurrentValue.ModelPath, _voskSettings.CurrentValue.VoskModel);
        _log.ModelDownloaded();
    }

    public async Task<bool> ValidateModelFilesAsync(VoskSettings settings)
    {
        if (!Directory.Exists(settings.ModelPath))
        {
            _log.ModelFolderNotFound();
            return false;
        }

        var checksumFilePath = GetChecksumFileName(settings.VoskModel);
        if (!File.Exists(checksumFilePath))
        {
            _log.ChecksumFileMissing();
            return false;
        }

        _log.FilesFound();
        var combinedChecksumOnFile = await File.ReadAllBytesAsync(checksumFilePath);
        var combinedChecksum = await GetCombinedChecksumAsync(settings.ModelPath);
        var isValid = combinedChecksumOnFile.SequenceEqual(combinedChecksum);
        if (isValid)
        {
            _log.ChecksumSuccess();
        }
        else
        {
            _log.ChecksumFailed();
        }

        return isValid;
    }

    private async Task<byte[]> GetCombinedChecksumAsync(string modelPath)
    {
        var files = Directory.GetFiles(modelPath, "*", SearchOption.AllDirectories);
        var checksumTasks = files.Select(async x => await ChecksumFileAsync(x));
        var checksums = await checksumTasks.WhenAll();
        var combinedChecksum = MD5.HashData(checksums.SelectMany(static x => x).ToArray());
        _log.CombinedChecksum(combinedChecksum);
        return combinedChecksum;
    }

    private async Task<byte[]> ChecksumFileAsync(string filePath)
    {
        _log.CreatingFileChecksum(filePath);
        await using var file = File.OpenRead(filePath);
        var checksum = await MD5.HashDataAsync(file);
        _log.CheckSumCreated(filePath, checksum);
        return checksum;
    }

    private async Task CreateChecksumFileAsync(string modelPath, VoskModel voskModel)
    {
        _log.GettingCombinedChecksum();
        var checksumFilePath = GetChecksumFileName(voskModel);
        var combinedChecksum = await GetCombinedChecksumAsync(modelPath);
        await File.WriteAllBytesAsync(checksumFilePath, combinedChecksum);
        _log.CombinedChecksum(combinedChecksum);
    }

    private async Task DownloadModelFileAsync(string modelPath, VoskModel voskModel)
    {
        if (Directory.Exists(modelPath))
        {
            _log.ClearingOldDirectory();
            Directory.Delete(modelPath, true);
        }

        Directory.CreateDirectory(modelPath);

        await using var file = await _downloader.GetVoskModelZip(voskModel);
        _log.ExtractingZip();
        ZipFile.ExtractToDirectory(file, modelPath);
        _log.ZipExtracted();
        RemoveTopLevelFolder(modelPath);
        _log.RemovedTopLevelFolder();
    }

    //The zip file contains a redundant top level folder,
    //so we can just remove it rather than try and manage their naming for it
    private static void RemoveTopLevelFolder(string modelPath)
    {
        var topLevel = Directory.GetDirectories(modelPath)[0];
        var files = Directory.GetFiles(topLevel, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var relativePath = file.Substring(topLevel.Length + 1);
            var destinationPath = Path.Combine(modelPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? throw new UnreachableException());

            File.Move(file, destinationPath);
        }

        Directory.Delete(topLevel, true);
    }

    private static string GetChecksumFileName(VoskModel voskModel)
    {
        return Path.Combine(VoskSettings.VoskFolder, $"checksum_{voskModel}");
    }
}

internal static partial class VoskModelDownloaderLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Checking the state of model files")]
    public static partial void CheckingModelFilesState(this ILogger log);

    [LoggerMessage(EventId = 2,
                   Level = LogLevel.Information,
                   Message = "The model is already downloaded and file checksum success")]
    public static partial void FilesAlreadyOk(this ILogger log);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Checksum failed, initializing model download")]
    public static partial void DownloadingModel(this ILogger log);

    [LoggerMessage(EventId = 4,
                   Level = LogLevel.Information,
                   Message = "Model downloaded successfully, creating checksum file")]
    public static partial void CreatingChecksum(this ILogger log);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Initialization of model successful")]
    public static partial void ModelDownloaded(this ILogger log);

    [LoggerMessage(EventId = 6,
                   Level = LogLevel.Debug,
                   Message = "Model folder was not found, model files invalid at the moment")]
    public static partial void ModelFolderNotFound(this ILogger log);

    [LoggerMessage(EventId = 7,
                   Level = LogLevel.Warning,
                   Message = "The model directory exists but the checksum file is missing, model invalid")]
    public static partial void ChecksumFileMissing(this ILogger log);

    [LoggerMessage(EventId = 8,
                   Level = LogLevel.Information,
                   Message = "Folder and checksum file found, checking checksum")]
    public static partial void FilesFound(this ILogger log);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Checksum matched folder contents")]
    public static partial void ChecksumSuccess(this ILogger log);

    [LoggerMessage(EventId = 10, Level = LogLevel.Warning, Message = "Checksum failed in the folder, re-downloading.")]
    public static partial void ChecksumFailed(this ILogger log);

    [LoggerMessage(EventId = 11, Level = LogLevel.Trace, Message = "Getting combined checksum of all files")]
    public static partial void GettingCombinedChecksum(this ILogger log);

    [LoggerMessage(EventId = 12,
                   Level = LogLevel.Debug,
                   Message = "Created the combined checksum of all files ({combinedChecksum})")]
    public static partial void CombinedChecksum(this ILogger log, Base64Logger combinedChecksum);

    [LoggerMessage(EventId = 13, Level = LogLevel.Trace, Message = "Working on checksum of file ({filePath})")]
    public static partial void CreatingFileChecksum(this ILogger log, string filepath);

    [LoggerMessage(EventId = 14,
                   Level = LogLevel.Trace,
                   Message = "Checksum of file ({filePath}) resolved as ({checksum})")]
    public static partial void CheckSumCreated(this ILogger log, string filepath, Base64Logger checksum);

    [LoggerMessage(EventId = 15,
                   Level = LogLevel.Information,
                   Message = "Old directory found for download, clearing old directory in preparation of download")]
    public static partial void ClearingOldDirectory(this ILogger log);

    [LoggerMessage(EventId = 16, Level = LogLevel.Information, Message = "file download started and zip is extracted")]
    public static partial void ExtractingZip(this ILogger log);

    [LoggerMessage(EventId = 17,
                   Level = LogLevel.Information,
                   Message = "file downloaded successfully and zip fully extracted.")]
    public static partial void ZipExtracted(this ILogger log);

    [LoggerMessage(EventId = 18, Level = LogLevel.Information, Message = "Removed top level folder of the zip file")]
    public static partial void RemovedTopLevelFolder(this ILogger log);
}
