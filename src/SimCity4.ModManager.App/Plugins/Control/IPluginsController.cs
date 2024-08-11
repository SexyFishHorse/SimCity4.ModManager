using System.Collections.Generic;
using System.ComponentModel;

namespace SimCity4.ModManager.App.Plugins.Control;

public interface IPluginsController
{
    ICollection<SimCity4.ModManager.App.Model.Plugin> Plugins { get; set; }

    void Add(SimCity4.ModManager.App.Model.Plugin plugin);

    void Update(SimCity4.ModManager.App.Model.Plugin plugin);

    void Remove(SimCity4.ModManager.App.Model.Plugin plugin);

    void UninstallPlugin(SimCity4.ModManager.App.Model.Plugin plugin);

    int IdentifyNewPlugins(BackgroundWorker backgroundWorker);

    int NumberOfRecognizedPlugins(SimCity4.ModManager.App.Model.UserFolder userFolder);

    int RemoveEmptyPlugins();

    void QuarantineFiles(IEnumerable<SimCity4.ModManager.App.Model.PluginFile> files);

    void UnquarantineFiles(IEnumerable<SimCity4.ModManager.App.Model.PluginFile> files);

    void RemoveFilesFromPlugins(ICollection<string> deletedFilePaths);

    void ReloadPlugins();

    int UpdateKnownPlugins(BackgroundWorker backgroundWorker);

    IEnumerable<Asser.Sc4Buddy.Server.Api.V1.Models.Plugin> CheckDependencies(SimCity4.ModManager.App.Model.UserFolder userFolder, BackgroundWorker backgroundWorker);
}