using System.Collections.Generic;
using System.IO;

namespace SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs;

public class ReadmeFilesEventArgs(FileInfo fileInfo, IEnumerable<FileInfo> readmeFiles)
    : InstallPluginEventArgs(fileInfo)
{
    public IEnumerable<FileInfo> ReadmeFiles { get; private set; } = readmeFiles;
}