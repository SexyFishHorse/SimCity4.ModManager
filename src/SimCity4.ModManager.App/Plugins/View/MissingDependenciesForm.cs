using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Asser.Sc4Buddy.Server.Api.V1.Models;

namespace SimCity4.ModManager.App.Plugins.View;

using SimCity4.ModManager.App.Resources;

public partial class MissingDependenciesForm : Form
{
    private IEnumerable<Plugin> missingDependencies;

    private Plugin selectedItem;

    private IList<Plugin> visitedDependencies;

    public MissingDependenciesForm()
    {
        InitializeComponent();
    }

    public IEnumerable<Plugin> MissingDependencies
    {
        get
        {
            return missingDependencies;
        }

        set
        {
            missingDependencies = value;
            visitedDependencies = new List<Plugin>();

            UpdateListView();
        }
    }

    private void UpdateListView()
    {
        dependencyListView.BeginUpdate();
        dependencyListView.Items.Clear();

        foreach (var remotePlugin in MissingDependencies)
        {
            var item = new SimCity4.ModManager.App.UI.Elements.ListViewItemWithObjectValue<Plugin>(remotePlugin.Name, remotePlugin);
            item.SubItems.Add(remotePlugin.Author);
            item.SubItems.Add(remotePlugin.Link);
            item.SubItems.Add(visitedDependencies.Contains(remotePlugin) ? LocalizationStrings.Visited : string.Empty);
            dependencyListView.Items.Add(item);
        }

        dependencyListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        dependencyListView.EndUpdate();
    }

    private void DependencyListViewSelectedIndexChanged(object sender, EventArgs e)
    {
        if (dependencyListView.SelectedItems.Count > 0)
        {
            var item =
                ((SimCity4.ModManager.App.UI.Elements.ListViewItemWithObjectValue<Plugin>)dependencyListView.SelectedItems[0])
                .Value;

            selectedItem = item;

            goToDownloadButton.Enabled = true;
        }
        else
        {
            goToDownloadButton.Enabled = false;
        }
    }

    private void GoToDownloadButtonClick(object sender, EventArgs e)
    {
        Process.Start(selectedItem.Link);
        if (!visitedDependencies.Contains(selectedItem))
        {
            visitedDependencies.Add(selectedItem);
        }

        UpdateListView();
    }
}