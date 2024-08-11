using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimCity4.ModManager.App.DataAccess;

public class Entities(string storageLocation) : IEntities
{
    private const string PluginGroupsFilename = "PluginGroups.json";

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public ICollection<SimCity4.ModManager.App.Model.PluginGroup> Groups { get; private set; }

    private string StorageLocation { get; } = storageLocation;

    private string PluginGroupsLocation => Path.Combine(StorageLocation, PluginGroupsFilename);

    public void SaveChanges()
    {
        StoreDataInFile(Groups, PluginGroupsLocation);
    }

    public void RevertChanges(SimCity4.ModManager.App.Model.ModelBase entityObject)
    {
    }

    public void RevertChanges(IEnumerable<SimCity4.ModManager.App.Model.ModelBase> entityCollection)
    {
    }

    public void Dispose()
    {
    }

    public void LoadAllEntitiesFromDisc()
    {
        Groups = new Collection<SimCity4.ModManager.App.Model.PluginGroup>();

        LoadPluginGroups(PluginGroupsLocation);
    }

    private static void StoreDataInFile(IEnumerable<SimCity4.ModManager.App.Model.ModelBase> data, string dataLocation)
    {
        if (!data.Any())
        {
            Log.Info($"Empty collection for {dataLocation}, skipping storage.");

            return;
        }

        var fileInfo = new FileInfo(dataLocation);
        if (fileInfo.DirectoryName == null)
        {
            throw new DirectoryNotFoundException(
                $"The location string {dataLocation} does not contain a directory name.");
        }

        Directory.CreateDirectory(fileInfo.DirectoryName);

        using (var writer = new StreamWriter(dataLocation))
        {
            var json = JsonConvert.SerializeObject(data);
            writer.Write(json);
        }
    }

    private void LoadPluginGroups(string fileLocation)
    {
        if (!File.Exists(fileLocation))
        {
            return;
        }

        using (var reader = new StreamReader(fileLocation))
        {
            var json = reader.ReadToEnd();

            dynamic dynamicGroups = JArray.Parse(json);

            foreach (var dynamicGroup in dynamicGroups)
            {
                var pluginGroup = new SimCity4.ModManager.App.Model.PluginGroup((Guid)dynamicGroup.Id) { Name = dynamicGroup.Name };
                var groupPlugins = new Collection<SimCity4.ModManager.App.Model.Plugin>();

                foreach (var pluginId in dynamicGroup.PluginIds)
                {
                    groupPlugins.Add(new SimCity4.ModManager.App.Model.Plugin((Guid)pluginId));
                }

                pluginGroup.Plugins = groupPlugins;
                Groups.Add(pluginGroup);
            }
        }
    }
}