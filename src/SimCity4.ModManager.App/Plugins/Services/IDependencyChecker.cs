using System.Collections.Generic;
using System.ComponentModel;

namespace SimCity4.ModManager.App.Plugins.Services;

public interface IDependencyChecker
{
    IEnumerable<Asser.Sc4Buddy.Server.Api.V1.Models.Plugin> CheckDependencies(
        SimCity4.ModManager.App.Model.UserFolder userFolder,
        ICollection<SimCity4.ModManager.App.Model.Plugin> mainUserFolderPlugins,
        BackgroundWorker backgroundWorker);
}