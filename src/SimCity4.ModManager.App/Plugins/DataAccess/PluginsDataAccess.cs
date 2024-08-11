using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimCity4.ModManager.App.Plugins.DataAccess;

public class PluginsDataAccess(
    SimCity4.ModManager.App.Model.UserFolder userFolder,
    SimCity4.ModManager.App.Utils.IJsonFileWriter writer,
    SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController)
{
    public const string Filename = "Plugins.json";

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public SimCity4.ModManager.App.Model.UserFolder UserFolder { get; set; } = userFolder;

    public ICollection<SimCity4.ModManager.App.Model.Plugin> LoadPlugins()
    {
        Log.Info($"Loading plugins for user folder {UserFolder.FolderPath}.");

        var path = Path.Combine(UserFolder.PluginFolderPath, Filename);
        var fileInfo = new FileInfo(path);

        var plugins = new Collection<SimCity4.ModManager.App.Model.Plugin>();

        if (!fileInfo.Exists)
        {
            return plugins;
        }

        try
        {
            using (var reader = new StreamReader(fileInfo.OpenRead()))
            {
                var json = reader.ReadToEnd();
                dynamic pluginsJson = JArray.Parse(json);

                foreach (var pluginJson in pluginsJson)
                {
                    var groupName = pluginJson.Group.ToString();
                    var plugin = new SimCity4.ModManager.App.Model.Plugin((Guid)pluginJson.Id)
                    {
                        Author = pluginJson.Author,
                        Description = pluginJson.Description,
                        Name = pluginJson.Name,
                        Link = pluginJson.Link,
                        PluginGroup = pluginGroupController.Groups.FirstOrDefault(x => x.Name == groupName)
                    };

                    if (pluginJson.RemotePluginId != null && pluginJson.RemotePluginId != Guid.Empty.ToString() && !string.IsNullOrWhiteSpace(pluginJson.RemotePluginId.ToString()))
                    {
                        plugin.RemotePlugin = new Asser.Sc4Buddy.Server.Api.V1.Models.Plugin
                        {
                            Id = pluginJson.RemotePluginId
                        };
                    }

                    foreach (var fileJson in pluginJson.PluginFiles)
                    {
                        var file = new SimCity4.ModManager.App.Model.PluginFile((Guid)fileJson.Id)
                        {
                            Checksum = fileJson.Checksum,
                            Path = fileJson.Path,
                            QuarantinedFile = fileJson.QuarantinedFile
                        };
                        plugin.PluginFiles.Add(file);
                    }

                    plugins.Add(plugin);
                }
            }
        }
        catch (JsonReaderException exception)
        {
            Log.Error($"Error reading json from {fileInfo.FullName}", exception);
        }

        return plugins;
    }

    public void SavePlugins(IEnumerable<SimCity4.ModManager.App.Model.Plugin> plugins, SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        Log.Info($"Save plugins for user folder {UserFolder.FolderPath}.");

        var fileInfo = new FileInfo(Path.Combine(userFolder.PluginFolderPath, Filename));

        writer.WriteToFile(fileInfo, plugins);
    }
}