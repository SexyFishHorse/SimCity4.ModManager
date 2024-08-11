using System.Collections.Generic;

namespace SimCity4.ModManager.App.Plugins.Control;

public class PluginGroupController(SimCity4.ModManager.App.DataAccess.IEntities entities)
{
    public ICollection<SimCity4.ModManager.App.Model.PluginGroup> Groups
    {
        get
        {
            return entities.Groups;
        }
    }

    public void Delete(SimCity4.ModManager.App.Model.PluginGroup pluginGroup)
    {
        Groups.Remove(pluginGroup);
        SaveChanges();
    }

    public void SaveChanges()
    {
        entities.SaveChanges();
    }

    public void Add(SimCity4.ModManager.App.Model.PluginGroup pluginGroup)
    {
        entities.Groups.Add(pluginGroup);
        SaveChanges();
    }
}