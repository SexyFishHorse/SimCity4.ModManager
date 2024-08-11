using System.IO;

namespace SimCity4.ModManager.App.UserFolders.Control;

public class UserFolderController(SimCity4.ModManager.App.UserFolders.DataAccess.IUserFolderDataAccess userFolderDataAccess) : IUserFolderController
{
    public void Update(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        userFolderDataAccess.SaveUserFolder(userFolder);
    }

    public SimCity4.ModManager.App.Model.UserFolder LoadUserFolder(string path)
    {
        try
        {
            return userFolderDataAccess.LoadUserFolder(path);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }
}