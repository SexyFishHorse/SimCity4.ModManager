using System;
using System.IO;

namespace SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs;

public class InstallPluginEventArgs(FileInfo fileInfo) : EventArgs
{
    public FileInfo FileInfo { get; private set; } = fileInfo;
}