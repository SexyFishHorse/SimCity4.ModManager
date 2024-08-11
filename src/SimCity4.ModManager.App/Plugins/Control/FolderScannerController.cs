using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using log4net;
using Md5ChecksumUtility = SimCity4.ModManager.IO.Md5ChecksumUtility;

namespace SimCity4.ModManager.App.Plugins.Control;

public class FolderScannerController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public event EventHandler NewFilesFound;

    public List<string> NewFiles { get; private set; }

    public bool ScanFolder(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        try
        {
            var folderScanner = new SimCity4.ModManager.App.UserFolders.Control.FolderScanner(userFolder);

            if (!folderScanner.ScanFolderForNewFiles())
            {
                return false;
            }

            NewFiles = folderScanner.NewFiles.ToList();

            NewFilesFound?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error during folder scan: {ex}");

            return false;
        }
    }

    public int AutoGroupKnownFiles(SimCity4.ModManager.App.Model.UserFolder userFolder, IPluginsController pluginController, SimCity4.ModManager.App.Plugins.Services.IPluginMatcher pluginMatcher, BackgroundWorker backgroundWorker)
    {
        var files = new Collection<SimCity4.ModManager.App.Model.PluginFile>();
        var numFiles = NewFiles.Count;
        var filesProcessed = 0.0;

        Log.Info("Converting new files to plugin objects");
        foreach (var file in NewFiles)
        {
            files.Add(new SimCity4.ModManager.App.Model.PluginFile { Path = file, Checksum = Md5ChecksumUtility.ToHex(SimCity4.ModManager.IO.Md5ChecksumUtility.CalculateChecksum(file)) });
        }

        Log.Info("Reloading plugin data from the server");
        pluginMatcher.ReloadData();

        Log.Info("Matching files");
        var filesAndPlugins = pluginMatcher.GetMostLikelyPluginForEachFile(files, backgroundWorker);

        var plugins = new Collection<SimCity4.ModManager.App.Model.Plugin>();

        Log.Info("Merging results.");
        foreach (var fileAndPlugin in filesAndPlugins)
        {
            if (backgroundWorker.CancellationPending)
            {
                return 0;
            }

            var plugin = new SimCity4.ModManager.App.Model.Plugin(fileAndPlugin.Value.Id);
            if (!plugins.Contains(plugin))
            {
                plugin.Name = fileAndPlugin.Value.Name;
                plugin.Author = fileAndPlugin.Value.Author;
                plugin.Link = fileAndPlugin.Value.Link;
                plugin.Description = fileAndPlugin.Value.Description;
                plugin.PluginFiles.Add(fileAndPlugin.Key);
                plugins.Add(plugin);
            }
            else
            {
                plugins.First(x => x.Id == fileAndPlugin.Value.Id).PluginFiles.Add(fileAndPlugin.Key);
            }

            NewFiles.Remove(fileAndPlugin.Key.Path);
            filesProcessed++;

            backgroundWorker.ReportProgress(
                ((int)Math.Floor(filesProcessed / numFiles) * 5) + 95,
                $"Updated {filesProcessed} of {numFiles} files.");
        }

        Log.Info("Saving found plugins.");
        foreach (var plugin in plugins)
        {
            pluginController.Add(plugin);
        }

        pluginController.ReloadPlugins();

        Log.Info("Done with auto grouping plugins.");
        return plugins.Count;
    }
}