using System.Windows.Forms;

namespace SimCity4.ModManager.App.View.Elements;

public class PluginListViewItem(SimCity4.ModManager.App.Model.Plugin plugin, ListViewGroup group) : ListViewItem(plugin.Name, group)
{
    public SimCity4.ModManager.App.Model.Plugin Plugin { get; private set; } = plugin;
}