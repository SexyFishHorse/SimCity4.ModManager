using System;
using System.Linq;
using System.Windows.Forms;

namespace SimCity4.ModManager.App.Plugins.View;

public partial class EnterPluginInformationForm : Form
{
    private readonly SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController;

    private readonly SimCity4.ModManager.App.Model.UserFolder userFolder;

    private SimCity4.ModManager.App.Model.Plugin plugin;

    public EnterPluginInformationForm(SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController, SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        InitializeComponent();

        this.pluginGroupController = pluginGroupController;
        this.userFolder = userFolder;
    }

    public SimCity4.ModManager.App.Model.Plugin Plugin
    {
        private get
        {
            return plugin;
        }

        set
        {
            plugin = value;
            ClearForm();
            nameTextBox.Text = plugin.Name;
            pluginNameLabel.Text = plugin.Name;
            authorTextBox.Text = plugin.Author;
            descriptionTextBox.Text = plugin.Description;

            if (plugin.Link != null)
            {
                linkTextBox.Text = plugin.Link;
            }

            if (plugin.PluginGroup != null)
            {
                groupComboBox.Text = plugin.PluginGroup.Name;
            }

            installedFilesListView.BeginUpdate();

            installedFilesListView.Items.Clear();
            foreach (var pluginFile in value.PluginFiles.Where(x => x.QuarantinedFile == null))
            {
                installedFilesListView.Items.Add(pluginFile.Path.Substring(userFolder.PluginFolderPath.Length + 1));
            }

            installedFilesListView.Columns[0].Width = -2;
            installedFilesListView.EndUpdate();
        }
    }

    public SimCity4.ModManager.App.Model.Plugin TempPlugin { get; set; }

    private void ClearForm()
    {
        nameTextBox.Text = string.Empty;
        descriptionTextBox.Text = string.Empty;
        authorTextBox.Text = string.Empty;
        linkTextBox.Text = string.Empty;
        groupComboBox.SelectedIndex = -1;
    }

    private void CancelButtonClick(object sender, EventArgs e)
    {
        Close();
    }

    private void OkButtonClick(object sender, EventArgs e)
    {
        var newPlugin = new SimCity4.ModManager.App.Model.Plugin(plugin.Id);

        var oldGroup = Plugin.PluginGroup;
        newPlugin.Name = nameTextBox.Text.Trim();
        newPlugin.Author = authorTextBox.Text.Trim();
        newPlugin.Description = descriptionTextBox.Text.Trim();
        newPlugin.PluginFiles = Plugin.PluginFiles;
        newPlugin.RemotePlugin = Plugin.RemotePlugin;

        if (!string.IsNullOrWhiteSpace(linkTextBox.Text))
        {
            newPlugin.Link = linkTextBox.Text.Trim();
        }

        newPlugin.PluginGroup = GetOrCreateGroup();

        if (oldGroup != null && !oldGroup.Equals(Plugin.PluginGroup))
        {
            oldGroup.Plugins.Remove(Plugin);
            pluginGroupController.SaveChanges();
        }

        if (newPlugin.PluginGroup != null)
        {
            newPlugin.PluginGroup.Plugins.Add(Plugin);
            pluginGroupController.SaveChanges();
        }

        TempPlugin = newPlugin;

        Close();
    }

    private SimCity4.ModManager.App.Model.PluginGroup GetOrCreateGroup()
    {
        if (groupComboBox.Text.Trim().Length <= 0)
        {
            return null;
        }

        var foundGroup =
            pluginGroupController.Groups.FirstOrDefault(
                x => x.Name.Equals(groupComboBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
        if (foundGroup != null)
        {
            return foundGroup;
        }

        var newGroup = new SimCity4.ModManager.App.Model.PluginGroup { Name = groupComboBox.Text };

        pluginGroupController.Add(newGroup);
        return newGroup;
    }

    private void EnterPluginInformationFormLoad(object sender, EventArgs e)
    {
        var groups = pluginGroupController.Groups;

        foreach (var pluginGroup in groups)
        {
            groupComboBox.Items.Add(new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Model.PluginGroup>(pluginGroup.Name, pluginGroup));
            groupComboBox.AutoCompleteCustomSource.Add(pluginGroup.Name);
        }
    }
}