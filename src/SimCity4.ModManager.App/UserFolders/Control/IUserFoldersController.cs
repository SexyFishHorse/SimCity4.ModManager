using System;
using System.Collections.Generic;

namespace SimCity4.ModManager.App.UserFolders.Control;

public interface IUserFoldersController
{
    ICollection<SimCity4.ModManager.App.Model.UserFolder> UserFolders { get; set; }

    SimCity4.ModManager.App.Model.UserFolder GetMainUserFolder();

    void Add(SimCity4.ModManager.App.Model.UserFolder userFolder);

    void Update(SimCity4.ModManager.App.Model.UserFolder userFolder);

    void Delete(SimCity4.ModManager.App.Model.UserFolder userFolder);

    bool ValidatePath(string path, Guid currentId);

    bool IsNotGameFolder(string path);

    bool ValidateAlias(string alias, Guid currentId);

    SimCity4.ModManager.App.Model.UserFolder GetUserFolderDataByPath(string path);
}