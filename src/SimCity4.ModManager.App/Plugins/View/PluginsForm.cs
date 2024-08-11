using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows.Forms;
using log4net;

namespace SimCity4.ModManager.App.Plugins.View;

using SimCity4.ModManager.App.Properties;
using SimCity4.ModManager.App.Resources;
using Plugin = SimCity4.ModManager.App.Model.Plugin;

public partial class PluginsForm : Form
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly SimCity4.ModManager.App.Plugins.Control.IPluginsController pluginsController;

    private readonly SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController;

    private readonly SimCity4.ModManager.App.Plugins.Services.IPluginMatcher pluginMatcher;

    private readonly SimCity4.ModManager.App.Model.UserFolder userFolder;

    private readonly SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController;

    private Plugin selectedPlugin;

    public PluginsForm(
        SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController,
        SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController,
        SimCity4.ModManager.App.Plugins.Control.IPluginsController pluginsController,
        SimCity4.ModManager.App.Model.UserFolder userFolder,
        SimCity4.ModManager.App.Plugins.Services.IPluginMatcher pluginMatcher)
    {
        this.pluginGroupController = pluginGroupController;
        this.userFoldersController = userFoldersController;
        this.pluginsController = pluginsController;

        if (!Directory.Exists(userFolder.FolderPath))
        {
            if (userFolder.IsMainFolder)
            {
                if (userFolder.Alias.Equals("?"))
                {
                    userFolder.Alias = LocalizationStrings.GameUserFolderName;
                }

                userFolder.FolderPath = SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation);
                userFoldersController.Update(userFolder);
            }
            else
            {
                throw new DirectoryNotFoundException(
                    "The plugin folder does not exist or you don't have access to one or more of the folders in the path.");
            }
        }

        this.userFolder = userFolder;
        this.pluginMatcher = pluginMatcher;
        InitializeComponent();
    }

    public void ReloadAndRepopulate()
    {
        pluginsController.ReloadPlugins();
        RepopulateInstalledPluginsListView();
    }

    private void UserFolderFormLoad(object sender, EventArgs e)
    {
        RepopulateInstalledPluginsListView();
    }

    private void RepopulateInstalledPluginsListView()
    {
        installedPluginsListView.BeginUpdate();
        installedPluginsListView.Items.Clear();
        installedPluginsListView.Groups.Clear();

        foreach (var pluginGroup in pluginGroupController.Groups.Where(x => x.Plugins.Any()))
        {
            installedPluginsListView.Groups.Add(pluginGroup.Id.ToString(), pluginGroup.Name);
        }

        foreach (var plugin in pluginsController.Plugins)
        {
            var groupId = plugin.PluginGroup == null ? string.Empty : plugin.PluginGroup.Id.ToString();
            var listViewItem = new SimCity4.ModManager.App.View.Elements.PluginListViewItem(plugin, installedPluginsListView.Groups[groupId]);

            installedPluginsListView.Items.Add(listViewItem);
        }

        installedPluginsListView.EndUpdate();
    }

    private void InstalledPluginsListViewSelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedItems = installedPluginsListView.SelectedItems;

        if (selectedItems.Count > 0)
        {
            splitContainer1.Panel2Collapsed = false;
            selectedPlugin = ((SimCity4.ModManager.App.View.Elements.PluginListViewItem)selectedItems[0]).Plugin;
            nameLabel.Text = selectedPlugin.Name;
            authorLabel.Text = selectedPlugin.Author;
            descriptionRichTextBox.Text = selectedPlugin.Description;
            if (selectedPlugin.Link != null)
            {
                linkLabel.Text = selectedPlugin.Link;
            }
            else
            {
                linkLabel.Text = string.Empty;
            }

            uninstallButton.Enabled = true;

            moveOrCopyButton.Enabled = true;
            disableFilesButton.Enabled = true;
            updateInfoButton.Enabled = true;

            pluginInfoSplitContainer.Panel2Collapsed = true;
        }
        else
        {
            splitContainer1.Panel2Collapsed = true;

            uninstallButton.Enabled = false;
            updateInfoButton.Enabled = false;
            moveOrCopyButton.Enabled = false;
            disableFilesButton.Enabled = false;
        }
    }

    private void UninstallButtonClick(object sender, EventArgs e)
    {
        var confirmation = MessageBox.Show(
            this,
            string.Format(LocalizationStrings.AreYouSureYouWantToUninstall, selectedPlugin.Name),
            LocalizationStrings.ConfirmUninstall,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirmation == DialogResult.No)
        {
            return;
        }

        pluginsController.UninstallPlugin(selectedPlugin);
        ClearPluginInformation();
        RepopulateInstalledPluginsListView();
    }

    private void ClearPluginInformation()
    {
        nameLabel.Text = string.Empty;
        authorLabel.Text = string.Empty;
        descriptionRichTextBox.Text = string.Empty;

        uninstallButton.Enabled = false;
        updateInfoButton.Enabled = false;
        splitContainer1.Panel2Collapsed = true;
    }

    private void LinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            var link = selectedPlugin.Link;
            Log.Info($"Launching browser: {link}");

            Process.Start(link);
        }
        catch (Exception ex)
        {
            Log.Warn("Unable to launch browser", ex);
            MessageBox.Show(
                this,
                string.Format(LocalizationStrings.UnableToLaunchBrowser, ex.Message),
                LocalizationStrings.ErrorDuringLaunchOfBrowser,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void UpdateInfoButtonClick(object sender, EventArgs e)
    {
        var infoDialog = new EnterPluginInformationForm(pluginGroupController, userFolder)
        {
            Plugin = selectedPlugin
        };

        if (infoDialog.ShowDialog(this) == DialogResult.OK)
        {
            var plugin = infoDialog.TempPlugin;
            pluginsController.Update(plugin);
        }

        RepopulateInstalledPluginsListView();
    }

    private void InstallToolStripMenuItemClick(object sender, EventArgs e)
    {
        var result = selectFilesToInstallDialog.ShowDialog(this);
        if (result == DialogResult.Cancel)
        {
            return;
        }

        var files = selectFilesToInstallDialog.FileNames;

        var confirmResult = MessageBox.Show(
            this,
            string.Format(LocalizationStrings.AreYouSureYouWantToInstallTheSelectedPlugins, files.Count()),
            LocalizationStrings.ConfirmInstallation,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1);

        if (confirmResult == DialogResult.No)
        {
            return;
        }

        new InstallPluginsForm(pluginsController, files, userFolder, pluginMatcher).ShowDialog(this);

        RepopulateInstalledPluginsListView();

        if (!NetworkInterface.GetIsNetworkAvailable() || !SimCity4.ModManager.App.Configuration.Settings.Get<bool>(SimCity4.ModManager.App.Configuration.Settings.Keys.AllowCheckForMissingDependencies))
        {
            return;
        }

        var scanForDependencies = MessageBox.Show(
            this,
            LocalizationStrings.WouldYouLikeToScanForMissingDependencies,
            LocalizationStrings.DependencyCheck,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1);

        if (scanForDependencies == DialogResult.Yes)
        {
            CheckForMissingDependenciesToolStripMenuItemClick(sender, e);
        }
    }

    private void ScanForNewPluginsToolStripMenuItemClick(object sender, EventArgs e)
    {
        new FolderScannerForm(
            new SimCity4.ModManager.App.Plugins.Control.FolderScannerController(),
            pluginsController,
            pluginGroupController,
            userFolder,
            pluginMatcher).ShowDialog(this);
        RepopulateInstalledPluginsListView();

        if (!NetworkInterface.GetIsNetworkAvailable() || !SimCity4.ModManager.App.Configuration.Settings.Get<bool>(SimCity4.ModManager.App.Configuration.Settings.Keys.AllowCheckForMissingDependencies))
        {
            return;
        }

        var scanForDependencies = MessageBox.Show(
            this,
            LocalizationStrings.WouldYouLikeToScanForMissingDependencies,
            LocalizationStrings.DependencyCheck,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1);

        if (scanForDependencies == DialogResult.Yes)
        {
            CheckForMissingDependenciesToolStripMenuItemClick(sender, e);
        }
    }

    private void ScanForNonpluginFilesToolStripMenuItemClick(object sender, EventArgs e)
    {
        var storageLocation = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Irradiated Games",
            "SimCity 4 Buddy",
            "Configuration");

        var nonPluginFilesScannerUi = new SimCity4.ModManager.App.View.Helpers.NonPluginFilesScannerUi(storageLocation) { UserFolder = userFolder };

        nonPluginFilesScannerUi.ScanForCandidates();

        if (!nonPluginFilesScannerUi.RemovalCandidateInfos.Any())
        {
            nonPluginFilesScannerUi.ShowThereAreNoEntitiesToRemoveDialog(this);
            return;
        }

        if (!nonPluginFilesScannerUi.ShowConfirmDialog(this))
        {
            return;
        }

        nonPluginFilesScannerUi.RemoveNonPluginFilesAndShowSummary(this);
    }

    private void IdentifyNewPluginsToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (!SimCity4.ModManager.App.Remote.Utils.ApiConnect.HasConnectionAndIsFeatureEnabled(SimCity4.ModManager.App.Configuration.Settings.Keys.DetectPlugins))
        {
            MessageBox.Show(
                this,
                Resources.This_feature_is_disabled_go_to_the_settings_if_you_want_to_enable_it,
                Resources.Feature_disabled,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        Log.Debug("Clicked identify new plugins");
        identifyPluginsBackgroundWorker.RunWorkerAsync();
        toolStripProgressBar.Visible = true;
        toolStripProgressBar.Value = 0;
        toolStripStatusLabel.Visible = true;
        toolStripStatusLabel.Text = Resources.PluginsForm_IdentifyNewPluginsToolStripMenuItemClick_Identifying_new_plugins;
    }

    private void UserFolderFormActivated(object sender, EventArgs e)
    {
        identifyNewPluginsToolStripMenuItem.Visible = SimCity4.ModManager.App.Configuration.Settings.Get<bool>(SimCity4.ModManager.App.Configuration.Settings.Keys.FetchInformationFromRemoteServer);
    }

    private void CheckForMissingDependenciesToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (!SimCity4.ModManager.App.Remote.Utils.ApiConnect.HasConnectionAndIsFeatureEnabled(SimCity4.ModManager.App.Configuration.Settings.Keys.AllowCheckForMissingDependencies))
        {
            MessageBox.Show(
                this,
                Resources.This_feature_is_disabled_go_to_the_settings_if_you_want_to_enable_it,
                Resources.Feature_disabled,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        Log.Debug("Clicked check for missing dependencies.");
        dependencyCheckerBackgroundWorker.RunWorkerAsync();
        toolStripProgressBar.Visible = true;
        toolStripProgressBar.Value = 0;
        toolStripStatusLabel.Visible = true;
        toolStripStatusLabel.Text = Resources.PluginsForm_CheckForMissingDependenciesToolStripMenuItemClick_Checking_for_missing_dependencies;
    }

    private void MoveOrCopyButtonClick(object sender, EventArgs e)
    {
        var dialog = new MoveOrCopyForm(
            userFolder,
            userFoldersController,
            pluginsController,
            pluginGroupController)
        {
            Plugin = selectedPlugin
        };
        dialog.PluginCopied += DialogOnPluginCopied;
        dialog.PluginMoved += DialogOnPluginMoved;
        dialog.ErrorDuringCopyOrMove += DialogOnErrorDuringCopyOrMove;
        dialog.ShowDialog(this);

        RepopulateInstalledPluginsListView();
    }

    private void DialogOnPluginMoved(object sender, EventArgs eventArgs)
    {
        MessageBox.Show(
            this,
            LocalizationStrings.PluginHasBeenSuccessfullyMoved,
            LocalizationStrings.PluginMoved,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void DialogOnErrorDuringCopyOrMove(object sender, EventArgs eventArgs)
    {
        MessageBox.Show(
            this,
            LocalizationStrings.ErrorDuringCopyOrMoveOperation,
            LocalizationStrings.ErrorDuringCopyingOrMovingPlugin,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private void DialogOnPluginCopied(object sender, EventArgs eventArgs)
    {
        MessageBox.Show(
            this,
            LocalizationStrings.PluginHasBeenSuccessfullyCopied,
            LocalizationStrings.PluginCopied,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void DisableFilesButtonClick(object sender, EventArgs e)
    {
        var dialog = new QuarantinedPluginFilesForm(selectedPlugin);

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            pluginsController.QuarantineFiles(dialog.QuarantinedFiles);
            pluginsController.UnquarantineFiles(dialog.UnquarantinedFiles);
        }
    }

    private void OpenInFileExplorerToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (Directory.Exists(userFolder.FolderPath))
        {
            Process.Start(userFolder.FolderPath);
        }
        else
        {
            MessageBox.Show(
                this,
                $"The directory \"{userFolder.FolderPath}\" doesn't appear to exist.",
                @"Directory not found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }
    }

    private void PluginsFormDragEnter(object sender, DragEventArgs e)
    {
        e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void PluginsFormDragDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        Log.Info("User dropped the following files into the plugins form window.");
        foreach (var file in files)
        {
            Log.Info(file);
        }

        if (MessageBox.Show(
                this,
                $"Are you sure you want to install the selected {files.Length} plugins in the user folder {userFolder.Alias}?",
                Resources.ConfirmInstallationOfPlugins,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
        {
            Log.Info($"Installing in user folder {userFolder.FolderPath}");

            var form = new InstallPluginsForm(pluginsController, files, userFolder, pluginMatcher);

            form.ShowDialog(this);

            RepopulateInstalledPluginsListView();
        }
        else
        {
            Log.Info("Drop installation was cancelled.");
        }
    }

    private void CloseButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void PluginsFormFormClosing(object sender, FormClosingEventArgs e)
    {
        e.Cancel = true;
        if (updateInfoBackgroundWorker.IsBusy)
        {
            if (MessageBox.Show(
                    this,
                    Resources.PluginsForm_PluginsFormFormClosing_The_application_is_still_updating_info_for_known_plugins__Close_anyway_,
                    Resources.PluginsForm_PluginsFormFormClosing_Confirm_cancellation,
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                updateInfoBackgroundWorker.CancelAsync();
            }
            else
            {
                return;
            }
        }

        if (identifyPluginsBackgroundWorker.IsBusy)
        {
            if (MessageBox.Show(
                    this,
                    Resources.PluginsForm_PluginsFormFormClosing_The_application_is_still_identifying_new_plugins__Close_anyway_,
                    Resources.PluginsForm_PluginsFormFormClosing_Confirm_cancellation,
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                identifyPluginsBackgroundWorker.CancelAsync();
            }
            else
            {
                return;
            }
        }

        if (dependencyCheckerBackgroundWorker.IsBusy)
        {
            if (MessageBox.Show(
                    this,
                    Resources.PluginsForm_PluginsFormFormClosing_The_application_is_still_checking_for_missing_dependencies__Close_anyway_,
                    Resources.PluginsForm_PluginsFormFormClosing_Confirm_cancellation,
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                dependencyCheckerBackgroundWorker.CancelAsync();
            }
            else
            {
                return;
            }
        }

        Hide();
    }

    private void UpdateInfoForKnownPluginsToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (!SimCity4.ModManager.App.Remote.Utils.ApiConnect.HasConnectionAndIsFeatureEnabled(SimCity4.ModManager.App.Configuration.Settings.Keys.FetchInformationFromRemoteServer))
        {
            MessageBox.Show(
                this,
                Resources.This_feature_is_disabled_go_to_the_settings_if_you_want_to_enable_it,
                Resources.Feature_disabled,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        Log.Debug("Clicked update info for known plugins");
        toolStripProgressBar.Visible = true;
        toolStripProgressBar.Value = 0;
        toolStripStatusLabel.Visible = true;
        toolStripStatusLabel.Text = Resources.PluginsForm_UpdateInfoForKnownPluginsToolStripMenuItemClick_Updating_info_for_known_plugins_from_the_server_;

        updateInfoBackgroundWorker.RunWorkerAsync();
    }

    private void UpdateInfoBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
    {
        Log.Debug("Starting update info background worker.");
        var numUpdated = pluginsController.UpdateKnownPlugins(sender as BackgroundWorker);

        e.Result = numUpdated;
    }

    private void BackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        toolStripProgressBar.Style = ProgressBarStyle.Continuous;
        toolStripProgressBar.Value = e.ProgressPercentage;
        toolStripStatusLabel.Text = e.UserState.ToString();
    }

    private void UpdateInfoBackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        Log.Debug("Update info background worker completed");
        toolStripProgressBar.Visible = false;
        toolStripProgressBar.Value = 0;
        toolStripStatusLabel.Visible = false;
        toolStripStatusLabel.Text = string.Empty;
        ReloadAndRepopulate();

        MessageBox.Show(
            this,
            string.Format(Resources.PluginsForm_UpdateInfoBackgroundWorkerRunWorkerCompleted_Updated_information_for__0__plugins_, e.Result),
            Resources.PluginsForm_UpdateInfoBackgroundWorkerRunWorkerCompleted_Update_complete,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void IdentifyPluginsBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
    {
        Log.Debug("Starting identify plugins background worker.");
        var numIdentified = pluginsController.IdentifyNewPlugins(sender as BackgroundWorker);

        e.Result = numIdentified;
    }

    private void IdentifyPluginsBackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        Log.Debug("Identify new plugins background worker completed");
        toolStripProgressBar.Visible = false;
        toolStripProgressBar.Value = 0;
        toolStripStatusLabel.Visible = false;
        toolStripStatusLabel.Text = string.Empty;
        ReloadAndRepopulate();

        MessageBox.Show(
            this,
            string.Format(Resources.PluginsForm_IdentifyPluginsBackgroundWorkerRunWorkerCompleted_Identified__0__new_plugins_, e.Result),
            Resources.PluginsForm_UpdateInfoBackgroundWorkerRunWorkerCompleted_Update_complete,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void DependencyCheckerBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
    {
        Log.Debug("Starting dependency checker background worker.");
        var missingDependencies = pluginsController.CheckDependencies(userFolder, sender as BackgroundWorker);

        e.Result = missingDependencies;
    }

    private void DependencyCheckerBackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        Log.Debug("Check dependencies background worker completed");
        toolStripProgressBar.Visible = false;
        toolStripProgressBar.Value = 0;
        toolStripStatusLabel.Visible = false;
        toolStripStatusLabel.Text = string.Empty;

        var missingDependencies = ((IEnumerable<Asser.Sc4Buddy.Server.Api.V1.Models.Plugin>)e.Result).ToList();

        if (missingDependencies.Any())
        {
            var dialog = new MissingDependenciesForm { MissingDependencies = missingDependencies };
            dialog.ShowDialog(this);
        }
        else
        {
            MessageBox.Show(
                this,
                Resources.PluginsForm_DependencyCheckerBackgroundWorkerRunWorkerCompleted_No_missing_dependencies_were_detected_,
                Resources.PluginsForm_DependencyCheckerBackgroundWorkerRunWorkerCompleted_No_missing_dependencies,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}