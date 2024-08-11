using System;
using System.Collections.Generic;
using System.IO;

namespace SimCity4.ModManager.App.Plugins.Installer;

public class UserFolderListener : IDisposable
{
    public UserFolderListener(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        UserFolder = userFolder;
        ChangedFiles = new List<string>();
        CreatedFiles = new List<string>();
        DeletedFiles = new List<string>();
        RenamedFiles = new Dictionary<string, string>();

        Watcher = new FileSystemWatcher(UserFolder.PluginFolderPath, "*")
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };
        Watcher.Changed += WatcherEventListener;
        Watcher.Created += WatcherEventListener;
        Watcher.Deleted += WatcherEventListener;
        Watcher.Renamed += WatcherEventListener;
    }

    public ICollection<string> ChangedFiles { get; private set; }

    public ICollection<string> CreatedFiles { get; private set; }

    public ICollection<string> DeletedFiles { get; private set; }

    public IDictionary<string, string> RenamedFiles { get; private set; }

    private SimCity4.ModManager.App.Model.UserFolder UserFolder { get; set; }

    private FileSystemWatcher Watcher { get; set; }

    public void Start()
    {
        Watcher.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        Watcher.Dispose();
    }

    private void WatcherEventListener(object sender, FileSystemEventArgs fileSystemEventArgs)
    {
        switch (fileSystemEventArgs.ChangeType)
        {
            case WatcherChangeTypes.Changed:
                ChangedFiles.Add(fileSystemEventArgs.FullPath);
                break;
            case WatcherChangeTypes.Created:
                CreatedFiles.Add(fileSystemEventArgs.FullPath);
                break;
            case WatcherChangeTypes.Deleted:
                DeletedFiles.Add(fileSystemEventArgs.FullPath);
                break;
            case WatcherChangeTypes.Renamed:
            {
                var renamedEventArgs = (RenamedEventArgs)fileSystemEventArgs;

                if (RenamedFiles.ContainsKey(renamedEventArgs.OldFullPath))
                {
                    RenamedFiles.Remove(renamedEventArgs.OldFullPath);
                }

                RenamedFiles.Add(renamedEventArgs.OldFullPath, renamedEventArgs.FullPath);
            }

                break;
        }
    }
}