﻿using Tests.Helpers;

using Willow.Core.Settings.Abstractions;
using Willow.Vosk.Abstractions;
using Willow.Vosk.Enums;
using Willow.Vosk.Registration;
using Willow.Vosk.Settings;

namespace Tests.Speech.SpeechServers;

[Collection("vosk")]
public sealed class VoskDownloaderTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IVoskModelInstaller _sut;
    private readonly ISettings<VoskSettings> _settings;

    public VoskDownloaderTests()
    {
        var _downloader = Substitute.For<IVoskModelDownloader>();
        var services = new ServiceCollection();
        VoskServerRegistrar.RegisterServices(services);
        services.AddLogging();
        services.AddSingleton(typeof(ISettings<>), typeof(SettingsMock<>));
        services.AddSingleton(_downloader);
        _provider = services.BuildServiceProvider();
        _settings = _provider.GetRequiredService<ISettings<VoskSettings>>();
        _sut = _provider.GetRequiredService<IVoskModelInstaller>();
        _downloader.GetVoskModelZip(Arg.Any<VoskModel>())
                   .Returns(_ =>
                       Task.FromResult((Stream)File.Open("Speech/SpeechServers/vosk-model-small-en-us-0.15.zip", FileMode.Open)));
        EnsureDeletedFolder();
    }

    [Fact]
    public async Task When_NoModelFilePresent_DownloadModel()
    {
        Directory.Exists(_settings.CurrentValue.ModelPath).Should().BeFalse();
        await _sut.EnsureExistsAsync();
        Directory.Exists(_settings.CurrentValue.ModelPath).Should().BeTrue();
        (await _sut.ValidateModelFilesAsync(_settings.CurrentValue)).Should().BeTrue();
    }

    [Fact]
    public async Task When_OneOfModelFilesDeleted_VerificationFails()
    {
        await _sut.EnsureExistsAsync();
        Directory.Exists(_settings.CurrentValue.ModelPath).Should().BeTrue();
        (await _sut.ValidateModelFilesAsync(_settings.CurrentValue)).Should().BeTrue();
        var files = GetAllFilesRecursively(_settings.CurrentValue.ModelPath);
        File.Delete(files[0]);
        (await _sut.ValidateModelFilesAsync(_settings.CurrentValue)).Should().BeFalse();
    }

    [Fact]
    public async Task When_OneOfModelFilesChanged_VerificationFails()
    {
        await _sut.EnsureExistsAsync();
        Directory.Exists(_settings.CurrentValue.ModelPath).Should().BeTrue();
        (await _sut.ValidateModelFilesAsync(_settings.CurrentValue)).Should().BeTrue();
        var files = GetAllFilesRecursively(_settings.CurrentValue.ModelPath);
        await File.WriteAllTextAsync(files[0],"hello World");

        (await _sut.ValidateModelFilesAsync(_settings.CurrentValue)).Should().BeFalse();
    }

    [Fact]
    public async Task When_VerificationFailed_EnsureExistsFixesIt()
    {
        await _sut.EnsureExistsAsync();
        Directory.Exists(_settings.CurrentValue.ModelPath).Should().BeTrue();
        (await _sut.ValidateModelFilesAsync(_settings.CurrentValue)).Should().BeTrue();
        var files = GetAllFilesRecursively(_settings.CurrentValue.ModelPath);
        File.Delete(files[0]);
        (await _sut.ValidateModelFilesAsync(_settings.CurrentValue)).Should().BeFalse();
        await _sut.EnsureExistsAsync();
        (await _sut.ValidateModelFilesAsync(_settings.CurrentValue)).Should().BeTrue();
    }

    public void Dispose()
    {
        EnsureDeletedFolder();

        _provider.Dispose();
    }

    private void EnsureDeletedFolder()
    {
        if (Directory.Exists(VoskSettings.VoskFolder))
        {
            Directory.Delete(VoskSettings.VoskFolder, true);
        }
    }

    private string[] GetAllFilesRecursively(string path)
    {
        return Directory.GetFiles(path, "*",
            new EnumerationOptions()
            {
                RecurseSubdirectories = true, MaxRecursionDepth = 5, ReturnSpecialDirectories = false,
            });
    }
}