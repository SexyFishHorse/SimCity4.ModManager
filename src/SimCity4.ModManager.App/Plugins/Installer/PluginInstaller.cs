using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimCity4.ModManager.App.Plugins.Installer;

public class PluginInstaller
{
    public IEnumerable<FileSystemInfo> PluginFiles { get; set; }

    public IEnumerable<FileInfo> Executables { get; set; }

    public IEnumerable<FileInfo> ReadmeFiles { get; set; }

    public IEnumerable<SimCity4.ModManager.App.Model.PluginFile> InstalledFiles { get; set; }

    private SimCity4.ModManager.App.Plugins.Installer.FileHandlers.BaseHandler FileHandler { get; set; }

    public void ExtractToTempFolder(FileInfo fileInfo, string tempFolderPath)
    {
        if (!fileInfo.Exists)
        {
            throw new ArgumentException("FileInfo must point to an existing file.");
        }

        switch (fileInfo.Extension.ToUpper())
        {
            case ".DAT":
                FileHandler = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.DatHandler();
                break;
            case ".ZIP":
                FileHandler = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.ArchiveHandler();
                break;
            case ".RAR":
                FileHandler = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.ArchiveHandler();
                break;
            default:
                throw new ArgumentException(@"FileInfo must point to either a dat, rar or zip file.", nameof(fileInfo));
        }

        FileHandler.TempFolder = tempFolderPath;
        FileHandler.FileInfo = fileInfo;

        var tempFiles = FileHandler.ExtractFilesToTemp().ToList();
        ReadmeFiles = GetReadmeFiles(tempFiles.Where(x => x is FileInfo).Cast<FileInfo>());
        Executables = GetExecutables(tempFiles);
        PluginFiles = GetPluginFiles(tempFiles);
    }

    public void MoveToUserFolder(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        InstalledFiles = FileHandler.MoveToPluginFolder(userFolder);
    }

    private static IEnumerable<FileSystemInfo> GetPluginFiles(IEnumerable<FileSystemInfo> tempFiles)
    {
        return tempFiles.Where(entry => entry is DirectoryInfo || SimCity4.ModManager.App.Plugins.Installer.FileHandlers.BaseHandler.IsPluginFile(entry.FullName)).ToList();
    }

    private static IEnumerable<FileInfo> GetExecutables(IEnumerable<FileSystemInfo> tempFiles)
    {
        return
            tempFiles.Where(x => x is FileInfo && x.Extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
                .Cast<FileInfo>()
                .ToList();
    }

    private static IEnumerable<FileInfo> GetReadmeFiles(IEnumerable<FileInfo> tempFiles)
    {
        var readmeExtensions = new[] { ".html", ".htm", ".mht", ".pdf", ".txt", ".rtf", ".doc", ".docx", ".odt" };
        var nonReadmeFilenames = new[] { "CLEANITOL", "REMOVELIST" };

        var readmeFiles = new List<FileInfo>();
        foreach (
            var file in
            tempFiles.Where(file => !nonReadmeFilenames.Any(x => file.Name.ToUpper().Contains(x))))
        {
            readmeFiles.AddRange(
                readmeExtensions.Where(
                        readmeExtension => file.Extension.Equals(readmeExtension, StringComparison.OrdinalIgnoreCase))
                    .Select(readmeExtension => file));
        }

        return readmeFiles;
    }
}