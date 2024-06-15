namespace SimCity4.ModManager.Domain;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.Win32;

public class GameFinder
{
    public List<(string name, string Path)> GetInstallations()
    {
        var result = new List<(string name, string Path)>();

        if (OperatingSystem.IsWindows())
        {
            if (CheckEaRegistryInfo(out var eaPath))
            {
                result.Add(("EA", eaPath));
            }

            if (CheckSteamRegistryInfo(out var steamPath))
            {
                result.Add(("Steam", steamPath));
            }
        }

        return result;
    }

    [SupportedOSPlatform("windows")]
    private static bool CheckEaRegistryInfo([NotNullWhen(true)] out string? exePath)
    {
        exePath = (string?)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\SimCity 4.exe", string.Empty, string.Empty);

        if (string.IsNullOrEmpty(exePath) == false && File.Exists(exePath))
        {
            return true;
        }

        exePath = null;

        return false;
    }

    [SupportedOSPlatform("windows")]
    private static bool CheckSteamRegistryInfo([NotNullWhen(true)] out string? exePath)
    {
        var installFolder = (string?)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 24780", "InstallLocation", string.Empty);

        if (string.IsNullOrEmpty(installFolder))
        {
            exePath = null;
            
            return false;
        }

        exePath = Path.Combine(installFolder, "Apps", "SimCity 4.exe");

        if (File.Exists(exePath))
        {
            return true;
        }

        exePath = null;

        return false;
    }
}
