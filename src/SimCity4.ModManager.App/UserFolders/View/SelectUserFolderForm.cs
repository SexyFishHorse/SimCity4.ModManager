using System.Windows.Forms;

namespace SimCity4.ModManager.App.UserFolders.View;

public partial class SelectUserFolderForm : Form
{
    public SelectUserFolderForm(SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController)
    {
        InitializeComponent();

        userFolderListView.BeginUpdate();
        foreach (var userFolder in userFoldersController.UserFolders)
        {
            userFolderListView.Items.Add(new SimCity4.ModManager.App.View.Elements.ListViewItemWithObjectValue<SimCity4.ModManager.App.Model.UserFolder>(userFolder.Alias, userFolder));
        }

        userFolderListView.EndUpdate();
    }

    public SimCity4.ModManager.App.Model.UserFolder UserFolder { get; set; }

    private void InstallButtonClick(object sender, System.EventArgs e)
    {
        UserFolder = ((SimCity4.ModManager.App.View.Elements.ListViewItemWithObjectValue<SimCity4.ModManager.App.Model.UserFolder>)userFolderListView.SelectedItems[0]).Value;
    }

    private void UserFolderListViewSelectedIndexChanged(object sender, System.EventArgs e)
    {
        installButton.Enabled = userFolderListView.SelectedItems.Count > 0;
    }
}