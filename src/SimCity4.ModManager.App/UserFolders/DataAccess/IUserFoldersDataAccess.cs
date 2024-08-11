using System.Collections.Generic;

namespace SimCity4.ModManager.App.UserFolders.DataAccess;

public interface IUserFoldersDataAccess
{
    ICollection<SimCity4.ModManager.App.Model.UserFolder> LoadUserFolders();

    void SaveUserFolders(IEnumerable<SimCity4.ModManager.App.Model.UserFolder> userFolders);
}