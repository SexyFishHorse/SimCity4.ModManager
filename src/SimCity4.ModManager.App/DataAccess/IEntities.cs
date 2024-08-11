using System;
using System.Collections.Generic;

namespace SimCity4.ModManager.App.DataAccess;

public interface IEntities : IDisposable
{
    ICollection<SimCity4.ModManager.App.Model.PluginGroup> Groups { get; }

    void SaveChanges();

    void RevertChanges(SimCity4.ModManager.App.Model.ModelBase entityObject);

    void RevertChanges(IEnumerable<SimCity4.ModManager.App.Model.ModelBase> entityCollection);
}