using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using Md5ChecksumUtility = SimCity4.ModManager.IO.Md5ChecksumUtility;

namespace SimCity4.ModManager.App.Plugins.View;

using SimCity4.ModManager.App.Properties;
using SimCity4.ModManager.App.Resources;

public partial class FolderScannerForm : Form
{
    private const int ErrorIconPadding = -18;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static ImageList imageList;

    private readonly SimCity4.ModManager.App.Plugins.Control.FolderScannerController folderScannerController;

    private readonly SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController;

    private readonly SimCity4.ModManager.App.Plugins.Services.IPluginMatcher pluginMatcher;

    private readonly SimCity4.ModManager.App.Plugins.Control.IPluginsController pluginsController;

    private readonly List<string> selectedFiles;

    private readonly SimCity4.ModManager.App.Model.UserFolder userFolder;

    private List<string> foundFiles;

    public FolderScannerForm(
        SimCity4.ModManager.App.Plugins.Control.FolderScannerController folderScannerController,
        SimCity4.ModManager.App.Plugins.Control.IPluginsController pluginsController,
        SimCity4.ModManager.App.Plugins.Control.PluginGroupController pluginGroupController,
        SimCity4.ModManager.App.Model.UserFolder userFolder,
        SimCity4.ModManager.App.Plugins.Services.IPluginMatcher pluginMatcher)
    {
        this.folderScannerController = folderScannerController;
        this.pluginsController = pluginsController;
        this.pluginGroupController = pluginGroupController;

        this.userFolder = userFolder;
        this.pluginMatcher = pluginMatcher;

        selectedFiles = new List<string>();

        InitializeComponent();

        fileScannerBackgroundWorker.DoWork += ScanFolder;
        fileScannerBackgroundWorker.ProgressChanged += FileScannerBackgroundWorkerOnProgressChanged;
        fileScannerBackgroundWorker.RunWorkerCompleted += delegate
        {
            SetFormEnabled(true);
            statusProgressBar.Visible = false;
            statusLabel.Visible = false;
            scanButton.Enabled = true;
        };

        folderScannerController.NewFilesFound += FolderScannerControllerOnNewFilesFound;

        newFilesTreeView.ImageList = ImageList;
        selectedFilesTreeView.ImageList = ImageList;
    }

    private static ImageList ImageList
    {
        get
        {
            if (imageList == null)
            {
                imageList = new ImageList();
                imageList.Images.Add("Folder", Resources.TreeView_Folder);
                imageList.Images.Add("FolderOpen", Resources.TreeView_FolderOpen);
                imageList.Images.Add("Leaf", Resources.TreeView_Leaf);
            }

            return imageList;
        }
    }

    private static void PopulateTreeViewWithPaths(TreeView treeView, IEnumerable<string> paths)
    {
        treeView.BeginUpdate();

        PopulateTreeViewCollectionWithPaths(treeView.Nodes, paths);

        treeView.EndUpdate();
    }

    private static void PopulateTreeViewCollectionWithPaths(
        TreeNodeCollection nodeCollection,
        IEnumerable<string> paths)
    {
        nodeCollection.Clear();

        foreach (var path in paths)
        {
            var nodes = path.Split(Path.DirectorySeparatorChar);
            var hasChildren = nodes.Length > 1;

            var node = nodes[0];

            var existingNodes = nodeCollection.Find(node, false);

            if (existingNodes.Length == 0)
            {
                var nodeToAdd = new TreeNode(node) { ImageKey = hasChildren ? "Folder" : "Leaf", Name = node };
                if (hasChildren)
                {
                    nodeToAdd.Nodes.Add("Loading", "Loading...");
                }

                nodeCollection.Add(nodeToAdd);
            }
        }
    }

    private void AddAllButtonClick(object sender, EventArgs e)
    {
        Log.Debug("Add all found files");
        foreach (var foundFile in foundFiles)
        {
            selectedFiles.Add(foundFile);
        }

        foundFiles.Clear();

        RepopulateViews();
        addButton.Enabled = false;
    }

    private void AddButtonClick(object sender, EventArgs e)
    {
        Log.Debug("Add selected files");

        var files = GetSelectedPaths(newFilesTreeView, out var nodeName);

        foreach (var file in files)
        {
            selectedFiles.Add(file);
            foundFiles.Remove(file);
        }

        newFilesTreeView.SelectedNode = null;
        RepopulateViews();

        nameTextBox.Text = nodeName;
        addButton.Enabled = false;
    }

    private void AutoGroupBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
    {
        Log.Debug("Starting auto group background worker.");
        var numPluginsFound = folderScannerController.AutoGroupKnownFiles(
            userFolder,
            pluginsController,
            pluginMatcher,
            sender as BackgroundWorker);

        e.Result = numPluginsFound;
    }

    private void AutoGroupBackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs args)
    {
        statusProgressBar.Style = ProgressBarStyle.Continuous;
        statusProgressBar.Value = args.ProgressPercentage;
        statusLabel.Text = args.UserState.ToString();
    }

    private void AutoGroupBackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
    {
        Log.Debug("Background worker completed.");
        SetFormEnabled(true);
        statusProgressBar.Visible = false;
        statusLabel.Visible = false;
        statusProgressBar.Value = 0;
        statusLabel.Text = string.Empty;
        RepopulateViews();

        if (args.Cancelled || args.Error != null)
        {
            return;
        }

        var result = (int)args.Result;

        var text = string.Format(
            Resources.FolderScannerForm_AutoGroupBackgroundWorkerRunWorkerCompleted_Found__0__plugin_s__,
            result);

        MessageBox.Show(
            this,
            text,
            Resources.FolderScannerForm_AutoGroupBackgroundWorkerRunWorkerCompleted_Result_of_auto_grouping,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void AutoGroupKnownPluginsClick(object sender, EventArgs e)
    {
        Log.Debug("Auto grouping known plugins");
        SetFormEnabled(false);
        statusProgressBar.Visible = true;
        statusProgressBar.Value = 0;
        statusLabel.Visible = true;
        statusLabel.Text =
            Resources.FolderScannerForm_AutoGroupBackgroundWorkerDoWork_Attempting_to_group_files_automatically_;

        autoGroupBackgroundWorker.RunWorkerAsync();
    }

    private void ClearInfoAndSelectedFilesForms()
    {
        Log.Debug("Clearing form and selected files");
        nameTextBox.Text = string.Empty;
        authorTextBox.Text = string.Empty;
        linkTextBox.Text = string.Empty;
        descriptionTextBox.Text = string.Empty;
        groupComboBox.Text = string.Empty;
        saveButton.Enabled = false;

        errorProvider1.Clear();
        removeButton.Enabled = false;
        removeAllButton.Enabled = false;
        selectedFiles.Clear();

        RepopulateViews();
    }

    private void CloseButtonClick(object sender, EventArgs e)
    {
        Log.Debug("Close form");
        try
        {
            fileScannerBackgroundWorker.CancelAsync();
        }
        catch (Exception ex)
        {
            Log.Warn("Exception during folder scanner form close", ex);
        }
    }

    private void FileScannerBackgroundWorkerOnProgressChanged(
        object sender,
        ProgressChangedEventArgs progressChangedEventArgs)
    {
        statusProgressBar.Value = progressChangedEventArgs.ProgressPercentage;
    }

    private IEnumerable<string> FilesFromNode(TreeNodeCollection nodes)
    {
        foreach (var node in nodes.Cast<TreeNode>())
        {
            if (node.Nodes.Count > 0)
            {
                foreach (var path in FilesFromNode(node.Nodes))
                {
                    yield return path;
                }
            }
            else
            {
                yield return node.FullPath;
            }
        }
    }

    private void FolderScannerControllerOnNewFilesFound(object sender, EventArgs eventArgs)
    {
        Log.Debug("New files found");
        var newFiles = folderScannerController.NewFiles;
        var filenames = newFiles.Select(x => x.Remove(0, userFolder.PluginFolderPath.Length + 1)).ToList();

        foundFiles = filenames;

        Invoke(new System.Windows.Forms.MethodInvoker(RepopulateViews));
    }

    private void FolderScannerFormFormClosing(object sender, FormClosingEventArgs e)
    {
        Log.Debug("Closing form");
        if (autoGroupBackgroundWorker.IsBusy)
        {
            if (MessageBox.Show(
                    Resources
                        .FolderScannerForm_FolderScannerFormFormClosing_Do_you_want_to_cancel_the_auto_grouping_of_plugins_,
                    LocalizationStrings.ConfirmCancellation,
                    MessageBoxButtons.YesNo)
                != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            Log.Info("Cancelling auto group as form was closed.");
            autoGroupBackgroundWorker.CancelAsync();
        }

        if (fileScannerBackgroundWorker.IsBusy)
        {
            if (MessageBox.Show(
                    Resources
                        .FolderScannerForm_FolderScannerFormFormClosing_Do_you_want_to_cancel_the_scanning_for_new_files_,
                    LocalizationStrings.ConfirmCancellation,
                    MessageBoxButtons.YesNo)
                == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            Log.Info("Cancelling folder scanner as form was closed.");
            fileScannerBackgroundWorker.CancelAsync();
        }
    }

    private void FolderScannerFormLoad(object sender, EventArgs e)
    {
        Log.Debug("Folder loaded");
        groupComboBox.BeginUpdate();

        foreach (var pluginGroup in pluginGroupController.Groups)
        {
            groupComboBox.Items.Add(new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Model.PluginGroup>(pluginGroup.Name, pluginGroup));
        }

        groupComboBox.EndUpdate();
    }

    private SimCity4.ModManager.App.Model.PluginGroup GetSelectedGroup()
    {
        var selectedText = groupComboBox.Text;

        if (string.IsNullOrWhiteSpace(selectedText))
        {
            return null;
        }

        var group =
            pluginGroupController.Groups.FirstOrDefault(
                x => x.Name.Equals(selectedText, StringComparison.OrdinalIgnoreCase));

        if (group == null)
        {
            group = new SimCity4.ModManager.App.Model.PluginGroup
            {
                Name = selectedText.Trim()
            };
            pluginGroupController.Add(group);
        }

        return group;
    }

    private IEnumerable<string> GetSelectedPaths(TreeView treeView, out string nodeName)
    {
        var selectedNode = treeView.SelectedNode;
        nodeName = selectedNode.Text;

        var files = new List<string>();

        if (selectedNode.Nodes.Count > 0)
        {
            files.AddRange(FilesFromNode(selectedNode.Nodes));
        }
        else
        {
            files.Add(selectedNode.FullPath);
        }

        return files;
    }

    private void NameTextBoxTextChanged(object sender, EventArgs e)
    {
        Log.Debug("Name text box value changed");
        ValidatePluginInfo(false);
    }

    private void NewFilesTreeViewAfterSelect(object sender, TreeViewEventArgs e)
    {
        Log.Debug("Selected file in new files view");
        if (newFilesTreeView.SelectedNode != null)
        {
            addButton.Enabled = true;
            addAllButton.Enabled = true;
        }
        else
        {
            addButton.Enabled = false;
            addAllButton.Enabled = false;
        }
    }

    private void RemoveAllButtonClick(object sender, EventArgs e)
    {
        Log.Debug("Remove all files");
        foreach (var file in selectedFiles)
        {
            foundFiles.Add(file);
        }

        selectedFiles.Clear();

        RepopulateViews();
        removeButton.Enabled = false;
    }

    private void RemoveButtonClick(object sender, EventArgs e)
    {
        Log.Debug("Removing selected files");
        var files = GetSelectedPaths(selectedFilesTreeView, out _);

        foreach (var file in files)
        {
            selectedFiles.Remove(file);
            foundFiles.Add(file);
        }

        RepopulateViews();

        removeButton.Enabled = false;
    }

    private void RepopulateViews()
    {
        Log.Info("Repopulating views");
        PopulateTreeViewWithPaths(newFilesTreeView, foundFiles);
        PopulateTreeViewWithPaths(selectedFilesTreeView, selectedFiles);

        addAllButton.Enabled = foundFiles.Any();
        removeAllButton.Enabled = selectedFiles.Any();

        newFilesTreeView.SelectedNode = null;
        selectedFilesTreeView.SelectedNode = null;
    }

    private void SaveButtonClick(object sender, EventArgs e)
    {
        Log.Debug("Saving plugin");
        if (!ValidatePluginInfo())
        {
            return;
        }

        var plugin = new SimCity4.ModManager.App.Model.Plugin
        {
            Name = nameTextBox.Text.Trim(),
            Author = authorTextBox.Text.Trim(),
            Link = linkTextBox.Text.Trim(),
            PluginGroup = GetSelectedGroup(),
            Description = descriptionTextBox.Text.Trim()
        };

        plugin.PluginGroup?.Plugins.Add(plugin);

        foreach (var path in GetSelectedPaths(selectedFilesTreeView, out _))
        {
            var pluginFile = new SimCity4.ModManager.App.Model.PluginFile
            {
                Path = Path.Combine(userFolder.PluginFolderPath, path),
                Checksum = Md5ChecksumUtility.ToHex(
                        SimCity4.ModManager.IO.Md5ChecksumUtility
                            .CalculateChecksum(Path.Combine(userFolder.PluginFolderPath, path))),
            };

            plugin.PluginFiles.Add(pluginFile);
        }

        pluginsController.Add(plugin);

        ClearInfoAndSelectedFilesForms();
    }

    private void ScanButtonClick(object sender, EventArgs e)
    {
        Log.Debug("Scanning folder");
        fileScannerBackgroundWorker.RunWorkerAsync();

        SetFormEnabled(false);
        statusProgressBar.Style = ProgressBarStyle.Marquee;
        statusProgressBar.Visible = true;
        statusLabel.Text = LocalizationStrings.ScandingFolderThisMayTakeAFewMinutesIfYouHaveAVeryLargePluginFolder;
        statusLabel.Visible = true;
    }

    private void ScanFolder(object sender, EventArgs e)
    {
        if (!folderScannerController.ScanFolder(userFolder))
        {
            Invoke(
                new System.Windows.Forms.MethodInvoker(
                    () =>
                    {
                        MessageBox.Show(
                            this,
                            LocalizationStrings.NoNewDeletedOrUpdatedFilesDetected,
                            LocalizationStrings.NoFileChangesDetected,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1);
                        Close();
                    }));
        }
    }

    private void SelectedFilesTreeViewAfterSelect(object sender, TreeViewEventArgs e)
    {
        Log.Debug("Selected files selection changed");
        removeButton.Enabled = selectedFilesTreeView.SelectedNode != null;
    }

    private void SetFormEnabled(bool enabled)
    {
        splitContainer1.Enabled = enabled;
        scanButton.Enabled = enabled;
        autoGroupKnownPluginsButton.Enabled = enabled;
    }

    private void TreeViewBeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
        Log.Debug($"Expanding node \"{e.Node.Name}\"");

        var treeView = (TreeView)sender;

        var triggerNode = treeView.Nodes.Find(e.Node.Name, true).First();
        triggerNode.Nodes.Clear();

        var nodesToAdd = foundFiles.Where(x => x.StartsWith(e.Node.FullPath));
        var trimmedNodes = nodesToAdd
            .Select(
                node => node.Replace(e.Node.FullPath + Path.DirectorySeparatorChar, string.Empty))
            .ToList();

        PopulateTreeViewCollectionWithPaths(triggerNode.Nodes, trimmedNodes);
    }

    private bool ValidatePluginInfo(bool showErrors = true)
    {
        Log.Debug("Validate plugin info");
        var errors = false;
        if (string.IsNullOrWhiteSpace(nameTextBox.Text))
        {
            if (showErrors)
            {
                errorProvider1.SetIconPadding(nameTextBox, ErrorIconPadding);
                errorProvider1.SetError(nameTextBox, "You must enter a name for the plugin");
            }

            errors = true;
        }

        if (selectedFiles.Any())
        {
            if (showErrors)
            {
                errorProvider1.SetIconPadding(selectedFilesTreeView, ErrorIconPadding);
                errorProvider1.SetError(selectedFilesTreeView, "You must select at least one file.");
            }

            errors = true;
        }

        saveButton.Enabled = !errors;

        return !errors;
    }
}