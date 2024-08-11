using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Asser.Sc4Buddy.Server.Api.V1.Client;
using log4net;

namespace SimCity4.ModManager.App.Application.View;

using SimCity4.ModManager.App.Properties;
using SimCity4.ModManager.App.Resources;

public partial class Sc4Buddy : Form
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ResourceManager localizationManager;

    private readonly SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController;

    private readonly SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController;

    private readonly SimCity4.ModManager.App.Plugins.Services.IPluginMatcher pluginMatcher;

    private readonly Collection<SimCity4.ModManager.App.UserFolders.View.UserFolderForm> userFolderForms;

    private SimCity4.ModManager.App.UserFolders.View.ManageUserFoldersForm manageUserFoldersForm;

    private AboutBox aboutBox;

    private SettingsForm settingsForm;

    private SimCity4.ModManager.App.UserFolders.View.SelectUserFolderForm selectUserFolderForm;

    private ChangelogForm changelogForm;

    public Sc4Buddy(
        SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController,
        SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController,
        SimCity4.ModManager.App.Plugins.Services.IPluginMatcher pluginMatcher)
    {
        this.userFoldersController = userFoldersController;
        this.pluginGroupController = pluginGroupController;
        this.pluginMatcher = pluginMatcher;

        InitializeComponent();

        userFolderForms = new Collection<SimCity4.ModManager.App.UserFolders.View.UserFolderForm>();

        localizationManager = new System.ComponentModel.ComponentResourceManager(typeof(Sc4Buddy));
    }

    private static SimCity4.ModManager.App.Model.UserFolder GetSelectedUserFolder(object sender)
    {
        return ((SimCity4.ModManager.App.View.Elements.UserFolderToolStripMenuItem)sender).UserFolder;
    }

    private void UserFolderComboBoxCheckSelectedValue(object sender, EventArgs e)
    {
        var selectUserFolderText = localizationManager.GetString("userFolderComboBox.Text");
        if (userFolderComboBox.SelectedItem == null
            || userFolderComboBox.Text.Equals(selectUserFolderText))
        {
            userFolderComboBox.Text = selectUserFolderText;
            userFolderComboBox.ForeColor = Color.Gray;
        }
        else
        {
            userFolderComboBox.ForeColor = Color.Black;
        }
    }

    private void UserFolderComboBoxDropDown(object sender, EventArgs e)
    {
        userFolderComboBox.ForeColor = Color.Black;
    }

    private void ManageFoldersToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (manageUserFoldersForm == null)
        {
            manageUserFoldersForm = new SimCity4.ModManager.App.UserFolders.View.ManageUserFoldersForm(userFoldersController);
        }

        manageUserFoldersForm.Show(this);

        RepopulateUserFolderRelatives();
    }

    private void RepopulateUserFolderRelatives()
    {
        Log.Info("Repopulating user folder lists");

        userFolderComboBox.BeginUpdate();
        userFolderComboBox.Items.Clear();

        var remove = userFoldersToolStripMenuItem.DropDownItems.OfType<SimCity4.ModManager.App.View.Elements.UserFolderToolStripMenuItem>().ToList();
        foreach (var item in remove)
        {
            userFoldersToolStripMenuItem.DropDownItems.Remove(item);
        }

        var insertIndex = 0;
        var comboboxIndex = 0;
        var startupFolderIndex = -1;
        foreach (var userFolder in userFoldersController.UserFolders)
        {
            if (!userFolder.IsMainFolder)
            {
                userFolderComboBox.Items.Add(new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Model.UserFolder>(userFolder.Alias, userFolder));

                if (userFolder.IsStartupFolder)
                {
                    startupFolderIndex = comboboxIndex;
                }

                comboboxIndex++;
            }

            if (userFolder.Alias.Equals("?"))
            {
                userFolder.Alias = LocalizationStrings.GameUserFolderName;
                userFoldersController.Update(userFolder);
            }

            userFoldersToolStripMenuItem.DropDownItems.Insert(insertIndex, new SimCity4.ModManager.App.View.Elements.UserFolderToolStripMenuItem(userFolder, UserFolderMenuItemClick));
            insertIndex++;
        }

        if (startupFolderIndex >= 0)
        {
            userFolderComboBox.SelectedIndex = startupFolderIndex;
        }
        else
        {
            userFolderComboBox.SelectedItem = null;
            userFolderComboBox.Text = localizationManager.GetString("userFolderComboBox.Text");
            userFolderComboBox.ForeColor = Color.Gray;
        }

        userFolderComboBox.EndUpdate();
    }

    private void ExitToolStripMenuItemClick(object sender, EventArgs e)
    {
        Log.Info("Application closing (menu item click)");
        Close();
    }

    private void Sc4BuddyLoad(object sender, EventArgs e)
    {
        RepopulateUserFolderRelatives();

        UpdateBackground();
    }

    private void UpdateBackground()
    {
        Bitmap image;
        switch (SimCity4.ModManager.App.Configuration.Settings.GetInt(SimCity4.ModManager.App.Configuration.Settings.Keys.Wallpaper))
        {
            case 13:
                image = Resources.Wallpaper13;
                break;
            case 12:
                image = Resources.Wallpaper12;
                break;
            case 11:
                image = Resources.Wallpaper11;
                break;
            case 10:
                image = Resources.Wallpaper10;
                break;
            case 9:
                image = Resources.Wallpaper9;
                break;
            case 8:
                image = Resources.Wallpaper8;
                break;
            case 7:
                image = Resources.Wallpaper7;
                break;
            case 6:
                image = Resources.Wallpaper6;
                break;
            case 5:
                image = Resources.Wallpaper5;
                break;
            case 4:
                image = Resources.Wallpaper4;
                break;
            case 3:
                image = Resources.Wallpaper3;
                break;
            case 2:
                image = Resources.Wallpaper2;
                break;
            default:
                image = Resources.Wallpaper1;
                break;
        }

        backgroundPanel.BackgroundImage = image;
    }

    private void UserFolderMenuItemClick(object sender, EventArgs e)
    {
        var userFolder = GetSelectedUserFolder(sender);

        var form = userFolderForms.FirstOrDefault(x => x.UserFolder.Id == userFolder.Id);

        if (form == null)
        {
            var client = new BuddyServerClient(SimCity4.ModManager.App.Remote.Utils.ApiConnect.GetClient());
            var dependencyChecker = new SimCity4.ModManager.App.Plugins.Services.DependencyChecker(client);
            form = new SimCity4.ModManager.App.UserFolders.View.UserFolderForm(
                userFolder,
                pluginGroupController,
                userFoldersController,
                pluginMatcher,
                new SimCity4.ModManager.App.Plugins.Control.PluginsController(
                    new SimCity4.ModManager.App.Plugins.DataAccess.PluginsDataAccess(userFolder, new SimCity4.ModManager.App.Utils.JsonFileWriter(), pluginGroupController),
                    new SimCity4.ModManager.App.Plugins.DataAccess.PluginsDataAccess(userFoldersController.GetMainUserFolder(), new SimCity4.ModManager.App.Utils.JsonFileWriter(), pluginGroupController),
                    userFolder,
                    new SimCity4.ModManager.App.Plugins.Services.PluginMatcher(client),
                    client,
                    dependencyChecker));
            userFolderForms.Add(form);
        }

        form.UpdateData();

        form.Show();
        form.Focus();
    }

    private void PlayButtonClick(object sender, EventArgs e)
    {
        Log.Info("Launching game");
        playButton.Enabled = false;
        playButton.Text = LocalizationStrings.StartingGame;
        playButton.ForeColor = Color.Gray;
        playButton.Update();

        SimCity4.ModManager.App.Model.UserFolder selectedUserFolder = null;
        if (userFolderComboBox.SelectedItem != null)
        {
            selectedUserFolder = ((SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Model.UserFolder>)userFolderComboBox.SelectedItem).Value;
            Log.Info(
                string.Format(
                    "Selected user folder is {0} (id: {1})",
                    selectedUserFolder.Alias,
                    selectedUserFolder.Id));
        }
        else
        {
            Log.Info("No user folder selected.");
        }

        var arguments = new SimCity4.ModManager.App.Application.Control.GameArgumentsHelper().GetArgumentString(selectedUserFolder);

        var gameProcessStartInfo = new ProcessStartInfo
        {
            FileName =
                Path.Combine(
                    SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation),
                    "Apps",
                    "SimCity 4.exe"),
            Arguments = arguments,
            WorkingDirectory = SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation)
        };

        var gameLauncher = new SimCity4.ModManager.App.Application.Control.GameLauncher(gameProcessStartInfo, SimCity4.ModManager.App.Configuration.LauncherSettings.GetInt(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.AutoSaveWaitTime));
        var gameLauncherThread = new Thread(gameLauncher.Start) { Name = "SC4Buddy AutoSaver" };

        gameLauncherThread.Start();

        Thread.Sleep(5000);

        playButton.Enabled = true;
        playButton.Text = localizationManager.GetString("playButton.Text");
        playButton.ForeColor = Color.Black;
    }

    private void SettingsToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (settingsForm == null)
        {
            settingsForm = new SettingsForm(userFoldersController);
        }

        settingsForm.ShowDialog(this);

        UpdateBackground();
    }

    private void SupportToolStripMenuItemClick(object sender, EventArgs e)
    {
        Process.Start(ConfigurationManager.AppSettings.Get("SupportWeblink"));
    }

    private void AboutToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (aboutBox == null)
        {
            aboutBox = new AboutBox();
        }

        aboutBox.ShowDialog(this);
    }

    private void BugsAndFeedbackToolStripMenuItemClick(object sender, EventArgs e)
    {
        Process.Start(ConfigurationManager.AppSettings.Get("BugReportWeblink"));
    }

    private void OpenLogFileToolStripMenuItemClick(object sender, EventArgs e)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Irradiated Games", "SimCity 4 Buddy", "Logs");

        var file = $"log-{DateTime.Now.ToString("yyyy-MM-dd")}.txt";

        var filePath = Path.Combine(path, file);

        if (!File.Exists(filePath))
        {
            MessageBox.Show(
                this,
                $"No log file was found. Check the folder ({path}) manually.",
                @"No logfile was found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        Process.Start(filePath);
    }

    private void Sc4BuddyDragDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        Log.Info("User dropped the following files into the main window.");
        foreach (var file in files)
        {
            Log.Info(file);
        }

        if (selectUserFolderForm == null)
        {
            selectUserFolderForm = new SimCity4.ModManager.App.UserFolders.View.SelectUserFolderForm(userFoldersController);
        }

        if (selectUserFolderForm.ShowDialog(this) != DialogResult.OK)
        {
            Log.Info("Drag and drop installation cancelled.");
            return;
        }

        var userFolder = selectUserFolderForm.UserFolder;
        Log.Info($"Installing in user folder {userFolder.FolderPath}");

        var client = new BuddyServerClient(SimCity4.ModManager.App.Remote.Utils.ApiConnect.GetClient());
        var dependencyChecker = new SimCity4.ModManager.App.Plugins.Services.DependencyChecker(client);
        var form = new SimCity4.ModManager.App.Plugins.View.InstallPluginsForm(
            new SimCity4.ModManager.App.Plugins.Control.PluginsController(
                new SimCity4.ModManager.App.Plugins.DataAccess.PluginsDataAccess(userFolder, new SimCity4.ModManager.App.Utils.JsonFileWriter(), pluginGroupController),
                new SimCity4.ModManager.App.Plugins.DataAccess.PluginsDataAccess(userFoldersController.GetMainUserFolder(), new SimCity4.ModManager.App.Utils.JsonFileWriter(), pluginGroupController),
                userFolder,
                new SimCity4.ModManager.App.Plugins.Services.PluginMatcher(client),
                client,
                dependencyChecker),
            files,
            userFolder,
            pluginMatcher);

        form.ShowDialog(this);
    }

    private void Sc4BuddyDragEnter(object sender, DragEventArgs e)
    {
        e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void ChangelogMenuItemClick(object sender, EventArgs e)
    {
        if (changelogForm == null)
        {
            changelogForm = new ChangelogForm();
        }

        changelogForm.ShowDialog(this);
    }
}