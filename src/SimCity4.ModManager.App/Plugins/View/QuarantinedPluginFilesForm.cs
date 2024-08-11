using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SimCity4.ModManager.App.Plugins.View;

public partial class QuarantinedPluginFilesForm : Form
{
    private readonly SimCity4.ModManager.App.Model.Plugin selectedPlugin;

    public QuarantinedPluginFilesForm(SimCity4.ModManager.App.Model.Plugin selectedPlugin)
    {
        this.selectedPlugin = selectedPlugin;

        InitializeComponent();
    }

    public IEnumerable<SimCity4.ModManager.App.Model.PluginFile> UnquarantinedFiles { get; set; }

    public IEnumerable<SimCity4.ModManager.App.Model.PluginFile> QuarantinedFiles { get; set; }

    private void CancelButtonClick(object sender, EventArgs e)
    {
        Close();
    }

    private void QuarantinedPluginFilesFormLoad(object sender, EventArgs e)
    {
        var enabledFiles = this.selectedPlugin.PluginFiles
            .Where(x => x.QuarantinedFile == null)
            .ToList();
        var disabledFiles = this.selectedPlugin.PluginFiles
            .Where(x => x.QuarantinedFile != null)
            .ToList();

        PopulateListView(activeFilesListView, enabledFiles);
        PopulateListView(disabledFilesListView, disabledFiles);
    }

    private void PopulateListView(ListView listView, List<SimCity4.ModManager.App.Model.PluginFile> files)
    {
        if (!files.Any())
        {
            return;
        }

        listView.BeginUpdate();
        listView.Items.Clear();
        foreach (var file in files)
        {
            var filename = new FileInfo(file.Path).Name;
            listView.Items.Add(new SimCity4.ModManager.App.UI.Elements.ListViewItemWithObjectValue<SimCity4.ModManager.App.Model.PluginFile>(filename, file));
        }

        listView.EndUpdate();
    }

    private void ActiveFilesListViewSelectedIndexChanged(object sender, EventArgs e)
    {
        disableButton.Enabled = activeFilesListView.SelectedItems.Count > 0;
    }

    private void DisabledFilesListViewSelectedIndexChanged(object sender, EventArgs e)
    {
        enableButton.Enabled = disabledFilesListView.SelectedItems.Count > 0;
    }

    private void DisableButtonClick(object sender, EventArgs e)
    {
        var items = activeFilesListView.SelectedItems;

        MoveItemsBetweenListViews(activeFilesListView, disabledFilesListView, items);
    }

    private void EnableButtonClick(object sender, EventArgs e)
    {
        var items = disabledFilesListView.SelectedItems;

        MoveItemsBetweenListViews(disabledFilesListView, activeFilesListView, items);
    }

    private void MoveItemsBetweenListViews(
        ListView originListView,
        ListView targetListView,
        IEnumerable items)
    {
        originListView.BeginUpdate();
        targetListView.BeginUpdate();

        foreach (SimCity4.ModManager.App.UI.Elements.ListViewItemWithObjectValue<SimCity4.ModManager.App.Model.PluginFile> item in items)
        {
            originListView.Items.Remove(item);
            targetListView.Items.Add(item);
        }

        targetListView.EndUpdate();
        originListView.EndUpdate();
    }

    private void OkButtonClick(object sender, EventArgs e)
    {
        var quarantined = disabledFilesListView.Items.Cast<SimCity4.ModManager.App.UI.Elements.ListViewItemWithObjectValue<SimCity4.ModManager.App.Model.PluginFile>>();
        var unquarantined = activeFilesListView.Items.Cast<SimCity4.ModManager.App.UI.Elements.ListViewItemWithObjectValue<SimCity4.ModManager.App.Model.PluginFile>>();

        QuarantinedFiles = quarantined.Select(x => x.Value);
        UnquarantinedFiles = unquarantined.Select(x => x.Value);

        Close();
    }
}