using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Forms;

namespace SimCity4.ModManager.App.UserFolders.View;

using SimCity4.ModManager.App.Resources;

public partial class ManageUserFoldersForm : Form
{
    public const int ErrorIconPadding = -18;

    private readonly ResourceManager localizationManager;

    private readonly SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController;

    public ManageUserFoldersForm(SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController)
    {
        this.userFoldersController = userFoldersController;

        InitializeComponent();

        localizationManager = new System.ComponentModel.ComponentResourceManager(typeof(ManageUserFoldersForm));
    }

    public UserFolderMode? Mode { get; set; }

    private SimCity4.ModManager.App.Model.UserFolder SelectedFolder { get; set; }

    private void UserFoldersFormLoad(object sender, EventArgs e)
    {
        ReloadUserFoldersListView();
    }

    private void ReloadUserFoldersListView()
    {
        var userFolders = userFoldersController.UserFolders.Where(x => !x.IsMainFolder);

        UserFoldersListView.BeginUpdate();
        UserFoldersListView.Items.Clear();
        foreach (var userFolder in userFolders)
        {
            UserFoldersListView.Items.Add(new SimCity4.ModManager.App.View.Elements.UserFolderListViewItem(userFolder));
        }

        UserFoldersListView.EndUpdate();
    }

    private void BrowseButtonClick(object sender, EventArgs e)
    {
        var result = pathBrowseDialog.ShowDialog(this);

        if (result == DialogResult.OK)
        {
            var path = pathBrowseDialog.SelectedPath;
            pathTextBox.Text = path;

            TryLoadUserFolderData(path);
        }
    }

    private void TryLoadUserFolderData(string path)
    {
        if (path == null || path == localizationManager.GetString("pathTextBox.Text"))
        {
            return;
        }

        var userFolder = userFoldersController.GetUserFolderDataByPath(path);
        if (userFolder != null)
        {
            if (string.IsNullOrWhiteSpace(aliasTextBox.Text))
            {
                aliasTextBox.Text = userFolder.Alias;
            }

            userFolder.IsMainFolder = false;
            startupFolderCheckbox.Checked = userFolder.IsStartupFolder;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(aliasTextBox.Text))
            {
                aliasTextBox.Text = new DirectoryInfo(path).Name;
            }
        }
    }

    private void UserFoldersListViewSelectedIndexChanged(object sender, EventArgs e)
    {
        errorProvider.Clear();

        if (UserFoldersListView.SelectedItems.Count > 0)
        {
            SelectedFolder = ((SimCity4.ModManager.App.View.Elements.UserFolderListViewItem)UserFoldersListView.SelectedItems[0]).UserFolder;

            pathTextBox.Text = SelectedFolder.FolderPath;
            pathBrowseDialog.SelectedPath = SelectedFolder.FolderPath;
            aliasTextBox.Text = SelectedFolder.Alias;
            startupFolderCheckbox.Checked = !SelectedFolder.IsMainFolder && SelectedFolder.IsStartupFolder;

            startupFolderCheckbox.Enabled = !SelectedFolder.IsMainFolder;
            EnableForm();
            removeButton.Enabled = true;
            Mode = UserFolderMode.Update;
        }
        else
        {
            EnableForm(false);
            SelectedFolder = null;

            pathTextBox.Text = string.Empty;
            aliasTextBox.Text = string.Empty;

            removeButton.Enabled = false;
        }
    }

    private void RemoveButtonClick(object sender, EventArgs e)
    {
        var text = LocalizationStrings.ConfirmUserFolderDeletionMessageBeforeFolderName + " \""
            + SelectedFolder.Alias + "\"? (" + LocalizationStrings.UserFolderContentIsNotDeleted + ")";
        var result = MessageBox.Show(
            this,
            text,
            LocalizationStrings.ConfirmUserFolderDeletion,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Exclamation,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
        {
            return;
        }

        userFoldersController.Delete(SelectedFolder);

        ResetForm();
        ReloadUserFoldersListView();
    }

    private void ResetForm()
    {
        ClearForm();
        EnableForm(false);

        addButton.Enabled = true;
        removeButton.Enabled = false;
    }

    private void EnableForm(bool enabled = true)
    {
        pathTextBox.Enabled = enabled;
        browseButton.Enabled = enabled;
        aliasTextBox.Enabled = enabled;
        startupFolderCheckbox.Enabled = enabled;
        saveButton.Enabled = enabled;
        cancelButton.Enabled = enabled;
    }

    private void ClearForm()
    {
        pathBrowseDialog.SelectedPath = string.Empty;
        pathTextBox.Text = string.Empty;
        aliasTextBox.Text = string.Empty;

        SelectedFolder = null;
        Mode = null;
    }

    private void AddButtonClick(object sender, EventArgs e)
    {
        ClearForm();
        EnableForm();

        Mode = UserFolderMode.Add;

        pathTextBox.Focus();
    }

    private void CloseButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void SaveButtonClick(object sender, EventArgs e)
    {
        errorProvider.Clear();

        var hasErrors = false;

        switch (Mode)
        {
            case UserFolderMode.Add:
                var newFolder = new SimCity4.ModManager.App.Model.UserFolder
                {
                    FolderPath = pathTextBox.Text,
                    Alias = aliasTextBox.Text,
                    IsStartupFolder = startupFolderCheckbox.Checked
                };

                var pathOk = !userFoldersController.ValidatePath(newFolder.FolderPath, Guid.NewGuid());
                var notMainFolder = !userFoldersController.IsNotGameFolder(newFolder.FolderPath);

                if (pathOk || notMainFolder)
                {
                    hasErrors = true;
                    errorProvider.SetIconPadding(pathTextBox, ErrorIconPadding);
                    errorProvider.SetError(pathTextBox, LocalizationStrings.PathError);
                }

                if (!userFoldersController.ValidateAlias(newFolder.Alias, Guid.Empty))
                {
                    hasErrors = true;
                    errorProvider.SetIconPadding(aliasTextBox, ErrorIconPadding);
                    errorProvider.SetError(aliasTextBox, LocalizationStrings.AliasError);
                }

                if (hasErrors)
                {
                    return;
                }

                userFoldersController.Add(newFolder);
                break;
            case UserFolderMode.Update:

                if (!userFoldersController.ValidatePath(pathTextBox.Text, SelectedFolder.Id))
                {
                    hasErrors = true;
                    errorProvider.SetIconPadding(pathTextBox, ErrorIconPadding);
                    errorProvider.SetError(pathTextBox, LocalizationStrings.PathError);
                }

                if (!userFoldersController.ValidateAlias(aliasTextBox.Text, SelectedFolder.Id))
                {
                    hasErrors = true;
                    errorProvider.SetIconPadding(aliasTextBox, ErrorIconPadding);
                    errorProvider.SetError(aliasTextBox, LocalizationStrings.AliasError);
                }

                if (hasErrors)
                {
                    return;
                }

                SelectedFolder.FolderPath = pathTextBox.Text;
                SelectedFolder.Alias = aliasTextBox.Text;
                SelectedFolder.IsStartupFolder = startupFolderCheckbox.Checked;

                userFoldersController.Update(SelectedFolder);
                break;
        }

        ResetForm();
        ReloadUserFoldersListView();
    }

    private void CancelButtonClick(object sender, EventArgs e)
    {
        ResetForm();
    }

    private void PathTextBoxLeave(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(pathTextBox.Text))
        {
            pathTextBox.Text = localizationManager.GetString("pathTextBox.Text");
        }
    }

    private void PathTextBoxEnter(object sender, EventArgs e)
    {
        if (pathTextBox.Text.Trim() == localizationManager.GetString("pathTextBox.Text"))
        {
            pathTextBox.Text = string.Empty;
        }
    }

    private void PathTextBoxTextChanged(object sender, EventArgs e)
    {
        var text = pathTextBox.Text.Trim();
        if (!pathTextBox.Focused)
        {
            if (string.IsNullOrEmpty(text))
            {
                pathTextBox.Text = localizationManager.GetString("pathTextBox.Text");
            }
        }

        pathTextBox.ForeColor = text.Equals(localizationManager.GetString("pathTextBox.Text"))
            ? Color.Gray
            : Color.Black;

        if (!string.IsNullOrEmpty(text))
        {
            TryLoadUserFolderData(pathTextBox.Text);
        }
    }

    private void ManageUserFoldersFormFormClosing(object sender, FormClosingEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}