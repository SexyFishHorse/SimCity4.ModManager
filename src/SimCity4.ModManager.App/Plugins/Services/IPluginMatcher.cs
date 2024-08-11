using System.Collections.Generic;
using System.ComponentModel;

namespace SimCity4.ModManager.App.Plugins.Services;

using Plugin = Asser.Sc4Buddy.Server.Api.V1.Models.Plugin;

public interface IPluginMatcher
{
    Plugin GetMostLikelyPluginForGroupOfFiles(IEnumerable<SimCity4.ModManager.App.Model.PluginFile> fileInfos);

    IDictionary<SimCity4.ModManager.App.Model.PluginFile, Plugin> GetMostLikelyPluginForEachFile(ICollection<SimCity4.ModManager.App.Model.PluginFile> files, BackgroundWorker backgroundWorker);

    void ReloadData();
}