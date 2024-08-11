using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ByteSizeLib;

namespace SimCity4.ModManager.App.UserFolders.View;

public partial class UserFolderForm : Form
{
    private readonly SimCity4.ModManager.App.Plugins.Control.IPluginsController pluginsController;

    private readonly SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController;

    private readonly SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController;

    private readonly SimCity4.ModManager.App.Plugins.Services.IPluginMatcher pluginMatcher;

    private SimCity4.ModManager.App.Plugins.View.PluginsForm pluginsForm;

    public UserFolderForm(
        SimCity4.ModManager.App.Model.UserFolder userFolder,
        SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController,
        SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController,
        SimCity4.ModManager.App.Plugins.Services.IPluginMatcher pluginMatcher,
        SimCity4.ModManager.App.Plugins.Control.IPluginsController pluginsController)
    {
        UserFolder = userFolder;
        this.pluginGroupController = pluginGroupController;
        this.userFoldersController = userFoldersController;
        this.pluginMatcher = pluginMatcher;
        this.pluginsController = pluginsController;
        InitializeComponent();
    }

    public SimCity4.ModManager.App.Model.UserFolder UserFolder { get; set; }

    public void UpdateData()
    {
        Text = UserFolder.Alias;
        numberOfPluginsLabel.Text = pluginsController.Plugins.Count.ToString(CultureInfo.InvariantCulture);

        var directoryInfo = new DirectoryInfo(UserFolder.PluginFolderPath);
        directoryInfo.Create();
        var files = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories);
        var size = files.Sum(fileInfo => fileInfo.Length);

        sizeOfPluginsLabel.Text = ByteSize.FromBytes(size).ToString("#.##");
    }

    private void ManagePluginsButtonClick(object sender, System.EventArgs e)
    {
        Hide();

        if (pluginsForm == null)
        {
            pluginsForm = new SimCity4.ModManager.App.Plugins.View.PluginsForm(
                pluginGroupController,
                userFoldersController,
                pluginsController,
                UserFolder,
                pluginMatcher);
        }

        pluginsForm.ReloadAndRepopulate();
        pluginsForm.Show(this);
    }

    private void UserFolderFormLoad(object sender, System.EventArgs e)
    {
        UpdateData();
    }

    private void UserFolderFormFormClosing(object sender, FormClosingEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}