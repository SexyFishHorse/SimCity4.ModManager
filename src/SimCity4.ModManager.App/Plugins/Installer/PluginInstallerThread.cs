using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using SharpCompress.Common;
using Md5ChecksumUtility = SimCity4.ModManager.IO.Md5ChecksumUtility;

namespace SimCity4.ModManager.App.Plugins.Installer;

using SimCity4.ModManager.App.Resources;

public class PluginInstallerThread(SimCity4.ModManager.App.Plugins.Control.IPluginsController pluginsController)
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public delegate void InstallPluginEventHandler(PluginInstallerThread sender, SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstallPluginEventArgs args);

    public delegate void InstallPluginFailureEventHandler(PluginInstallerThread sender, SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstallPluginFailureEventArgs args);

    public delegate void InstallProgressChangedEventHandler(PluginInstallerThread sender, SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstallProgressEventArgs args);

    public delegate void InstalledPluginEventHandler(PluginInstallerThread sender, SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstalledPluginEventArgs args);

    public delegate void ReadmeFilesFoundEventHandler(PluginInstallerThread sender, SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.ReadmeFilesEventArgs args);

    public delegate void NotPartOneOfMultipartDetectedEventHandler(
        PluginInstallerThread sender, SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstallPluginEventArgs args);

    public event InstallPluginEventHandler InstallingPlugin;

    public event InstalledPluginEventHandler PluginInstalled;

    public event InstallPluginFailureEventHandler PluginInstallFailed;

    public event InstallProgressChangedEventHandler InstallProgressChanged;

    public event EventHandler AllPluginsInstalled;

    public event ReadmeFilesFoundEventHandler ReadmeFilesFound;

    public event NotPartOneOfMultipartDetectedEventHandler NotPartOneOfMultipartDetected;

    public string[] FilesToInstall { get; set; }

    public SimCity4.ModManager.App.Plugins.View.InstallPluginsForm Form { get; set; }

    public SimCity4.ModManager.App.Model.UserFolder UserFolder { get; set; }

    public void Install()
    {
        var installer = new PluginInstaller();

        foreach (var file in FilesToInstall)
        {
            var fileInfo = new FileInfo(file);
            RaiseInstallPluginEvent(fileInfo);
            Log.Info("Installing plugin " + fileInfo.Name);

            try
            {
                var installedFiles = new List<SimCity4.ModManager.App.Model.PluginFile>();

                var randomFileName = Path.GetRandomFileName();
                randomFileName = randomFileName.Substring(0, randomFileName.Length - 4) + DateTime.UtcNow.Ticks;

                try
                {
                    installer.ExtractToTempFolder(
                        fileInfo, Path.Combine(Path.GetTempPath(), "SC4Buddy", randomFileName));
                }
                catch (MultiVolumeExtractionException)
                {
                    RaiseNotPartOneOfMultipartDetectedEvent(fileInfo);
                    continue;
                }

                RaiseInstallProgressEvent(fileInfo, 25, LocalizationStrings.FilesExtractedToTemporaryFolder);

                HandleReadmeFiles(fileInfo, installer);

                installedFiles.AddRange(HandleExecutableFiles(installer, UserFolder));

                installedFiles.AddRange(HandlePluginFiles(installer));

                if (!installedFiles.Any())
                {
                    var validExtensions = $"({string.Join(", ", SimCity4.ModManager.App.Plugins.Installer.FileHandlers.BaseHandler.PluginFileExtensions)})";
                    var errorMessage =
                        string.Format(
                            LocalizationStrings.ThePluginDigNotContainAnyValidFilesToInstall,
                            validExtensions);
                    RaisePluginInstallFailedEvent(fileInfo, errorMessage);
                    continue;
                }

                RaiseInstallProgressEvent(fileInfo, 75, LocalizationStrings.FilesMovedToUserFolder);

                var plugin = new SimCity4.ModManager.App.Model.Plugin { Name = new FileInfo(file).Name, PluginFiles = installedFiles };

                pluginsController.Add(plugin);

                RaisePluginInstalledEvent(fileInfo, plugin);

                Log.Info("Installation successfull.");
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(
                    "Unexpected exception during plugin install: [{0}] {1}",
                    new object[] { ex.GetType().Name, ex.Message });

                Log.Error("Installation failed", ex);

                RaisePluginInstallFailedEvent(fileInfo, errorMessage);
            }
        }

        RaiseAllPluginsInstalledEvent();
    }

    protected virtual void RaiseNotPartOneOfMultipartDetectedEvent(FileInfo fileInfo)
    {
        NotPartOneOfMultipartDetected?.Invoke(this, new SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstallPluginEventArgs(fileInfo));
    }

    protected virtual void RaiseInstallProgressEvent(FileInfo fileInfo, int progress, string message)
    {
        InstallProgressChanged?.Invoke(this, new SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstallProgressEventArgs(fileInfo, progress, message));
    }

    protected virtual void RaisePluginInstalledEvent(FileInfo fileInfo, SimCity4.ModManager.App.Model.Plugin plugin)
    {
        PluginInstalled?.Invoke(this, new SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstalledPluginEventArgs(fileInfo, plugin));
    }

    protected virtual void RaiseAllPluginsInstalledEvent()
    {
        AllPluginsInstalled?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void RaisePluginInstallFailedEvent(FileInfo fileInfo, string errorMessage)
    {
        PluginInstallFailed?.Invoke(this, new SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstallPluginFailureEventArgs(fileInfo, errorMessage));
    }

    protected virtual void RaiseInstallPluginEvent(FileInfo fileInfo)
    {
        InstallingPlugin?.Invoke(this, new SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.InstallPluginEventArgs(fileInfo));
    }

    protected virtual void RaiseReadmeFilesFoundEvent(FileInfo fileInfo, IEnumerable<FileInfo> readmeFiles)
    {
        ReadmeFilesFound?.Invoke(this, new SimCity4.ModManager.App.Plugins.Installer.InstallerEventArgs.ReadmeFilesEventArgs(fileInfo, readmeFiles));
    }

    private IEnumerable<SimCity4.ModManager.App.Model.PluginFile> HandlePluginFiles(PluginInstaller installer)
    {
        if (installer.PluginFiles.Any())
        {
            installer.MoveToUserFolder(UserFolder);
            return installer.InstalledFiles;
        }

        return new List<SimCity4.ModManager.App.Model.PluginFile>(0);
    }

    private IEnumerable<SimCity4.ModManager.App.Model.PluginFile> HandleExecutableFiles(PluginInstaller installer, SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        return installer.Executables.Any() ? RunExecutables(installer, userFolder) : new List<SimCity4.ModManager.App.Model.PluginFile>(0);
    }

    private void HandleReadmeFiles(FileInfo fileInfo, PluginInstaller installer)
    {
        if (installer.ReadmeFiles.Any())
        {
            RaiseReadmeFilesFoundEvent(fileInfo, installer.ReadmeFiles);
        }
    }

    private IEnumerable<SimCity4.ModManager.App.Model.PluginFile> RunExecutables(PluginInstaller installer, SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        var installedFiles = new List<SimCity4.ModManager.App.Model.PluginFile>();

        foreach (
            var executable in
            installer.Executables.Where(
                x => SimCity4.ModManager.App.Configuration.Settings.Get<bool>(SimCity4.ModManager.App.Configuration.Settings.Keys.AutoRunExecutablesDuringInstallation) || Form.AskToRunExecutable(x)))
        {
            using (var folderListener = new UserFolderListener(userFolder))
            {
                folderListener.Start();

                var process = Process.Start(executable.FullName);

                if (process != null)
                {
                    process.WaitForExit();
                }
                else
                {
                    Form.ShowInstallationDidNotStartDialog();
                }

                installedFiles.AddRange(HandleListenerFileChanges(folderListener));
            }
        }

        return installedFiles;
    }

    private IEnumerable<SimCity4.ModManager.App.Model.PluginFile> HandleListenerFileChanges(UserFolderListener folderListener)
    {
        var installedFiles = new List<SimCity4.ModManager.App.Model.PluginFile>();
        pluginsController.RemoveFilesFromPlugins(folderListener.DeletedFiles);

        installedFiles.AddRange(
            folderListener.CreatedFiles.Where(File.Exists).Select(
                file => new SimCity4.ModManager.App.Model.PluginFile { Path = file, Checksum = Md5ChecksumUtility.ToHex(SimCity4.ModManager.IO.Md5ChecksumUtility.CalculateChecksum(file)) }));

        pluginsController.RemoveFilesFromPlugins(folderListener.ChangedFiles);

        installedFiles.AddRange(
            folderListener.ChangedFiles.Where(File.Exists).Select(
                file => new SimCity4.ModManager.App.Model.PluginFile { Path = file, Checksum = Md5ChecksumUtility.ToHex(Md5ChecksumUtility.CalculateChecksum(file)) }));

        var oldPaths = new List<string>();
        var newPaths = new List<string>();
        foreach (var file in folderListener.RenamedFiles)
        {
            oldPaths.Add(file.Key);
            newPaths.Add(file.Value);
        }

        pluginsController.RemoveFilesFromPlugins(oldPaths);

        installedFiles.AddRange(
            newPaths.Where(File.Exists).Select(
                file => new SimCity4.ModManager.App.Model.PluginFile { Path = file, Checksum = Md5ChecksumUtility.ToHex(Md5ChecksumUtility.CalculateChecksum(file)) }));

        return installedFiles;
    }
}