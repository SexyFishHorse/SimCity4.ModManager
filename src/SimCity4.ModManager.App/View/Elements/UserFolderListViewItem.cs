using System.Windows.Forms;

namespace SimCity4.ModManager.App.View.Elements;

public class UserFolderListViewItem : ListViewItem
{
    public UserFolderListViewItem(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        Text = userFolder.Alias;
        Name = userFolder.Id.ToString();
        UserFolder = userFolder;
    }

    public SimCity4.ModManager.App.Model.UserFolder UserFolder { get; set; }
}