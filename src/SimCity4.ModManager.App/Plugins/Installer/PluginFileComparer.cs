using System;
using System.Collections.Generic;

namespace SimCity4.ModManager.App.Plugins.Installer;

public class PluginFileComparer : IEqualityComparer<SimCity4.ModManager.App.Model.PluginFile>
{
    public bool Equals(SimCity4.ModManager.App.Model.PluginFile x, SimCity4.ModManager.App.Model.PluginFile y)
    {
        return x.Path.Equals(y.Path, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(SimCity4.ModManager.App.Model.PluginFile obj)
    {
        return obj.Path.GetHashCode();
    }
}