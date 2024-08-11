using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Asser.Sc4Buddy.Server.Api.V1.Client;
using log4net;

namespace SimCity4.ModManager.App.Plugins.View;

public partial class MoveOrCopyForm : Form
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly SimCity4.ModManager.App.Model.UserFolder currentUserFolder;

    private readonly SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController;

    private readonly SimCity4.ModManager.App.Plugins.Control.IPluginsController pluginsController;

    private readonly SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController;

    private SimCity4.ModManager.App.Model.UserFolder selectedUserFolder;

    public MoveOrCopyForm(
        SimCity4.ModManager.App.Model.UserFolder currentUserFolder,
        SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController,
        SimCity4.ModManager.App.Plugins.Control.IPluginsController pluginsController,
        SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController)
    {
        this.currentUserFolder = currentUserFolder;
        this.userFoldersController = userFoldersController;
        this.pluginsController = pluginsController;
        this.pluginGroupController = pluginGroupController;

        InitializeComponent();
    }

    public event EventHandler PluginCopied;

    public event EventHandler PluginMoved;

    public event EventHandler ErrorDuringCopyOrMove;

    public SimCity4.ModManager.App.Model.Plugin Plugin { get; set; }

    protected virtual void OnPluginCopied()
    {
        var handler = PluginCopied;
        if (handler != null)
        {
            handler(this, EventArgs.Empty);
        }
    }

    protected virtual void OnPluginMoved()
    {
        var handler = PluginMoved;
        if (handler != null)
        {
            handler(this, EventArgs.Empty);
        }
    }

    protected virtual void OnErrorDuringCopyOrMove()
    {
        var handler = ErrorDuringCopyOrMove;
        if (handler != null)
        {
            handler(this, EventArgs.Empty);
        }
    }

    private void CancelButtonClick(object sender, EventArgs e)
    {
        Close();
    }

    private void MoveOrCopyFormLoad(object sender, EventArgs e)
    {
        userFolderListView.BeginUpdate();
        userFolderListView.Items.Clear();

        var userFolders = userFoldersController.UserFolders;

        foreach (var userFolder in userFolders.Where(userFolder => !userFolder.Equals(currentUserFolder)))
        {
            userFolderListView.Items.Add(new SimCity4.ModManager.App.View.Elements.ListViewItemWithObjectValue<SimCity4.ModManager.App.Model.UserFolder>(userFolder.Alias, userFolder));
        }

        userFolderListView.EndUpdate();
    }

    private void UserFolderListViewSelectedIndexChanged(object sender, EventArgs e)
    {
        if (userFolderListView.SelectedItems.Count > 0)
        {
            selectedUserFolder = ((SimCity4.ModManager.App.View.Elements.ListViewItemWithObjectValue<SimCity4.ModManager.App.Model.UserFolder>)userFolderListView.SelectedItems[0]).Value;

            moveButton.Enabled = true;
            copyButton.Enabled = true;
        }
        else
        {
            moveButton.Enabled = false;
            copyButton.Enabled = false;
        }
    }

    private void CopyButtonClick(object sender, EventArgs e)
    {
        var client = new BuddyServerClient(SimCity4.ModManager.App.Remote.Utils.ApiConnect.GetClient());
        var dependencyChecker = new SimCity4.ModManager.App.Plugins.Services.DependencyChecker(client);
        var copier = new SimCity4.ModManager.App.Plugins.Control.PluginCopier(
            pluginsController,
            new SimCity4.ModManager.App.Plugins.Control.PluginsController(
                new SimCity4.ModManager.App.Plugins.DataAccess.PluginsDataAccess(selectedUserFolder, new SimCity4.ModManager.App.Utils.JsonFileWriter(), pluginGroupController),
                new SimCity4.ModManager.App.Plugins.DataAccess.PluginsDataAccess(userFoldersController.GetMainUserFolder(), new SimCity4.ModManager.App.Utils.JsonFileWriter(), pluginGroupController),
                selectedUserFolder,
                new SimCity4.ModManager.App.Plugins.Services.PluginMatcher(client),
                client,
                dependencyChecker));
        try
        {
            copier.CopyPlugin(Plugin, currentUserFolder, selectedUserFolder);
            OnPluginCopied();
        }
        catch (Exception ex)
        {
            Log.Error(
                string.Format(
                    "Error during copy plugin {0} (id: {1}) to folder {2}",
                    Plugin.Name,
                    Plugin.Id,
                    selectedUserFolder.PluginFolderPath),
                ex);
            OnErrorDuringCopyOrMove();
        }

        Close();
    }

    private void MoveButtonClick(object sender, EventArgs e)
    {
        var client = new BuddyServerClient(SimCity4.ModManager.App.Remote.Utils.ApiConnect.GetClient());
        var dependencyChecker = new SimCity4.ModManager.App.Plugins.Services.DependencyChecker(client);
        var copier = new SimCity4.ModManager.App.Plugins.Control.PluginCopier(
            pluginsController,
            new SimCity4.ModManager.App.Plugins.Control.PluginsController(
                new SimCity4.ModManager.App.Plugins.DataAccess.PluginsDataAccess(selectedUserFolder, new SimCity4.ModManager.App.Utils.JsonFileWriter(), pluginGroupController),
                new SimCity4.ModManager.App.Plugins.DataAccess.PluginsDataAccess(userFoldersController.GetMainUserFolder(), new SimCity4.ModManager.App.Utils.JsonFileWriter(), pluginGroupController),
                selectedUserFolder,
                new SimCity4.ModManager.App.Plugins.Services.PluginMatcher(client),
                client,
                dependencyChecker));
        try
        {
            copier.MovePlugin(Plugin, currentUserFolder, selectedUserFolder);
            OnPluginMoved();
        }
        catch (Exception ex)
        {
            Log.Error(
                string.Format(
                    "Error during moving plugin {0} (id: {1}) to folder {2}",
                    Plugin.Name,
                    Plugin.Id,
                    selectedUserFolder.PluginFolderPath),
                ex);
            OnErrorDuringCopyOrMove();
        }

        Close();
    }
}