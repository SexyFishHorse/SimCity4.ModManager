namespace SimCity4.ModManager.App.UserFolders.Control;

public interface IUserFolderController
{
    void Update(SimCity4.ModManager.App.Model.UserFolder userFolder);

    SimCity4.ModManager.App.Model.UserFolder LoadUserFolder(string path);
}