using System.IO;

namespace SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs;

public class InstallProgressEventArgs(FileInfo fileInfo, int progress, string message)
    : InstallPluginEventArgs(fileInfo)
{
    public int Progress { get; private set; } = progress;

    public string Message { get; private set; } = message;
}