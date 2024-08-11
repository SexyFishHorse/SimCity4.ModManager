using System;
using System.IO;

namespace SimCity4.ModManager.App.Plugins.Control;

public class PluginCopier(IPluginsController pluginsController, IPluginsController targetController)
{
    public void MovePlugin(SimCity4.ModManager.App.Model.Plugin plugin, SimCity4.ModManager.App.Model.UserFolder originUserFolder, SimCity4.ModManager.App.Model.UserFolder targetUserFolder)
    {
        CopyPlugin(plugin, originUserFolder, targetUserFolder);

        pluginsController.UninstallPlugin(plugin);
    }

    public void CopyPlugin(SimCity4.ModManager.App.Model.Plugin plugin, SimCity4.ModManager.App.Model.UserFolder originUserFolder, SimCity4.ModManager.App.Model.UserFolder targetUserFolder)
    {
        var newPlugin = new SimCity4.ModManager.App.Model.Plugin
        {
            Name = plugin.Name,
            Link = plugin.Link,
            Description = plugin.Description,
            Author = plugin.Author
        };

        foreach (var file in plugin.PluginFiles)
        {
            newPlugin.PluginFiles.Add(CopyFile(file, originUserFolder, targetUserFolder));
        }

        targetController.Add(newPlugin);
    }

    private static SimCity4.ModManager.App.Model.PluginFile CopyFile(SimCity4.ModManager.App.Model.PluginFile pluginFile, SimCity4.ModManager.App.Model.UserFolder originUserFolder, SimCity4.ModManager.App.Model.UserFolder targetUserFolder)
    {
        var currentPath = pluginFile.Path;
        var relativeFilePath = currentPath.Remove(0, originUserFolder.PluginFolderPath.Length + 1);

        var newFilePath = Path.Combine(targetUserFolder.PluginFolderPath, relativeFilePath);
        var newDirectoryPath = targetUserFolder.PluginFolderPath;
        if (relativeFilePath.Contains("\\"))
        {
            newDirectoryPath = Path.Combine(
                newDirectoryPath,
                relativeFilePath.Remove(relativeFilePath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase)));
        }

        Directory.CreateDirectory(newDirectoryPath);
        File.Copy(currentPath, newFilePath, true);

        return new SimCity4.ModManager.App.Model.PluginFile { Checksum = pluginFile.Checksum, Path = newFilePath };
    }
}