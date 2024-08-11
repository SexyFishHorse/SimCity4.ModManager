using System.IO;

namespace SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs;

public class InstallPluginFailureEventArgs(FileInfo fileInfo, string errorMessage)
    : InstallPluginEventArgs(fileInfo)
{
    public string ErrorMessage { get; private set; } = errorMessage;
}