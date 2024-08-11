using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Asser.Sc4Buddy.Server.Api.V1.Client;
using log4net;

namespace SimCity4.ModManager.App.Plugins.Services;

public class DependencyChecker(BuddyServerClient buddyServerClient) : IDependencyChecker
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public IEnumerable<Asser.Sc4Buddy.Server.Api.V1.Models.Plugin> CheckDependencies(SimCity4.ModManager.App.Model.UserFolder userFolder, ICollection<SimCity4.ModManager.App.Model.Plugin> mainUserFolderPlugins, BackgroundWorker backgroundWorker)
    {
        Log.Debug("Fetching all plugins from the server.");
        var allServerPlugins = buddyServerClient.GetAllPlugins().ToList();
        backgroundWorker.ReportProgress(20, "Fetched plugin information from the server");

        Log.Debug("Loading all dependencies.");
        var dependencies = new HashSet<Guid>();

        var userFolderPlugins = userFolder.Plugins.Where(x => x.RemotePlugin != null).ToList();

        foreach (var dependency in userFolderPlugins
                     .Select(plugin => allServerPlugins.FirstOrDefault(x => x.Id == plugin.RemotePluginId))
                     .Where(remotePlugin => remotePlugin != null)
                     .SelectMany(remotePlugin => remotePlugin.Dependencies))
        {
            dependencies.Add(dependency);
        }

        backgroundWorker.ReportProgress(40, "Loaded all dependencies. Checking the main user folder");

        var mainFolderPlugins = mainUserFolderPlugins.Where(x => x.RemotePlugin != null).ToList();

        Log.Debug($"{dependencies.Count} dependencies found in total.");
        Log.Debug($"Looping through {mainFolderPlugins.Count} plugins in the main user folder.");

        foreach (var plugin in mainFolderPlugins.Where(x => x.RemotePluginId.HasValue))
        {
            // ReSharper disable once PossibleInvalidOperationException Validated in foreach statement
            dependencies.Remove(plugin.RemotePluginId.Value);
            Log.Debug($"Removed {plugin.Name} ({plugin.RemotePluginId}) as a dependency");
        }

        backgroundWorker.ReportProgress(60, "Checking the user folder");

        Log.Debug($"{dependencies.Count} dependencies remaining. Looping through {userFolderPlugins.Count} plugins in the user folder.");

        foreach (var plugin in userFolderPlugins)
        {
            // ReSharper disable once PossibleInvalidOperationException validated when userFolderPlugins was created
            dependencies.Remove(plugin.RemotePluginId.Value);
            Log.Debug($"Removed {plugin.Name} ({plugin.RemotePluginId}) as a dependency");
        }

        backgroundWorker.ReportProgress(80, "Loading data for missing dependencies");
        Log.Debug($"{dependencies.Count} dependencies remaining. Loading plugin data for said plugins.");

        return dependencies.Select(dependency => allServerPlugins.First(plugin => plugin.Id == dependency));
    }
}