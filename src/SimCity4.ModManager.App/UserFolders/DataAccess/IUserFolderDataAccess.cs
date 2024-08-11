namespace SimCity4.ModManager.App.UserFolders.DataAccess;

public interface IUserFolderDataAccess
{
    SimCity4.ModManager.App.Model.UserFolder LoadUserFolder(string path);

    void SaveUserFolder(SimCity4.ModManager.App.Model.UserFolder userFolder);
}