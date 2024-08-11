using System.Collections.Generic;
using Asser.Sc4Buddy.Server.Api.V1.Models;

namespace SimCity4.ModManager.App.Plugins.View;

public class RemotePluginComparer : IEqualityComparer<Plugin>
{
    public bool Equals(Plugin x, Plugin y)
    {
        return x.Id == y.Id;
    }

    public int GetHashCode(Plugin obj)
    {
        return obj.GetHashCode();
    }
}