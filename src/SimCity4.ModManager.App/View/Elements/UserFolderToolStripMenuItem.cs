using System;
using System.Windows.Forms;

namespace SimCity4.ModManager.App.View.Elements;

public class UserFolderToolStripMenuItem(SimCity4.ModManager.App.Model.UserFolder userFolder, EventHandler onClick)
    : ToolStripMenuItem(userFolder.Alias, null, onClick)
{
    public SimCity4.ModManager.App.Model.UserFolder UserFolder { get; set; } = userFolder;
}