using System.IO;

namespace SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs;

public class InstalledPluginEventArgs(FileInfo fileInfo, SimCity4.ModManager.App.Model.Plugin plugin) : InstallPluginEventArgs(fileInfo)
{
    public SimCity4.ModManager.App.Model.Plugin Plugin { get; private set; } = plugin;
}