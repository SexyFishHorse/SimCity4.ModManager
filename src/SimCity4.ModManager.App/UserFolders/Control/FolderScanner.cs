using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimCity4.ModManager.App.UserFolders.Control;

public class FolderScanner(SimCity4.ModManager.App.Model.UserFolder userFolder)
{
    public SimCity4.ModManager.App.Model.UserFolder UserFolder { get; private set; } = userFolder;

    public IEnumerable<string> NewFiles { get; private set; }

    public bool ScanFolderForNewFiles()
    {
        var entries = GetFiles();

        NewFiles = GetNewFiles(entries);

        return NewFiles.Any();
    }

    private IEnumerable<string> GetNewFiles(IEnumerable<string> entries)
    {
        return entries.Where(entry => !UserFolder.Plugins.Any(plugin => plugin.PluginFiles.Any(file => file.Path == entry)));
    }

    private IEnumerable<string> GetFiles()
    {
        return Directory.EnumerateFiles(UserFolder.PluginFolderPath, "*", SearchOption.AllDirectories)
            .Where(SimCity4.ModManager.App.Plugins.Installer.FileHandlers.BaseHandler.IsPluginFile);
    }
}