using Willow.WhisperServer.Enum;
using Willow.WhisperServer.Settings;

namespace Willow.WhisperServer.Extensions;

internal static class ModelInitializationCommandExtensions
{
    public static void InitializeWhisperModel(this PyModule scope, WhisperModelSettings settings)
    {
        var modelInitializationCommand = BuildModelInitializationCommand(settings);
        scope.Exec(modelInitializationCommand);
    }

    private static string BuildModelInitializationCommand(WhisperModelSettings settings)
    {
        var modelSizeOrPath = GetModelSizeOrPath(settings);
        var deviceIndex = FormatDeviceIndex(settings.DeviceIndex);

        return
            $"""
             model = WhisperModel(
                 model_size_or_path='{modelSizeOrPath}',
                 device='{settings.Device.ToString().ToLower()}',
                 device_index={deviceIndex},
                 compute_type='{settings.ComputeType.ToString().ToLower()}',
                 cpu_threads={settings.CpuThreads},
                 num_workers=1,
                 download_root='./models',
                 local_files_only=False)
             """;
    }

    private static string GetModelSizeOrPath(WhisperModelSettings settings)
    {
        var modelSize = settings.ModelSize.ToString().Replace("V", "-v").ToLower();
        var isEnglishOnlyCompatible =
            settings.ModelSize != ModelSize.LargeV1 && settings.ModelSize != ModelSize.LargeV2;
        return isEnglishOnlyCompatible && settings.EnglishOnly ? $"{modelSize}.en" : modelSize;
    }

    private static string FormatDeviceIndex(int[] deviceIndex)
    {
        return deviceIndex.Length > 0
                   ? "[" + string.Join(", ", deviceIndex.Select(item => item.ToString())) + "]"
                   : "0";
    }
}