using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimCity4.ModManager.App.UserFolders.Control;

public class TheUserFoldersController(
    SimCity4.ModManager.App.UserFolders.DataAccess.IUserFoldersDataAccess userFoldersDataAccess,
    IUserFolderController userFolderController)
    : IUserFoldersController
{
    public ICollection<SimCity4.ModManager.App.Model.UserFolder> UserFolders { get; set; } = userFoldersDataAccess.LoadUserFolders();

    public SimCity4.ModManager.App.Model.UserFolder GetMainUserFolder()
    {
        var folder = UserFolders.FirstOrDefault(x => x.IsMainFolder);

        if (folder != null)
        {
            return folder;
        }

        folder = new SimCity4.ModManager.App.Model.UserFolder(Guid.NewGuid())
        {
            Alias = "Main user folder",
            IsMainFolder = true,
            FolderPath = Path.Combine(
                SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation),
                SimCity4.ModManager.App.Model.UserFolder.PluginFolderName)
        };
        Add(folder);

        return folder;
    }

    public void Add(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        UpdateIsStartupFolder(userFolder);
        UserFolders.Add(userFolder);

        userFolderController.Update(userFolder);
        userFoldersDataAccess.SaveUserFolders(UserFolders);
    }

    public void Update(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        if (userFolder.IsStartupFolder)
        {
            foreach (var folder in UserFolders.Where(x => x.IsStartupFolder && x.Id != userFolder.Id))
            {
                folder.IsStartupFolder = false;
            }
        }

        userFolderController.Update(userFolder);
        userFoldersDataAccess.SaveUserFolders(UserFolders);
    }

    public void Delete(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        UserFolders.Remove(userFolder);

        userFoldersDataAccess.SaveUserFolders(UserFolders);
    }

    public bool ValidatePath(string path, Guid currentId)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (!Directory.Exists(path))
        {
            return false;
        }

        var collision = UserFolders
            .FirstOrDefault(x => x.FolderPath.Equals(path, StringComparison.OrdinalIgnoreCase));

        if (currentId == Guid.Empty)
        {
            return collision == null;
        }

        if (collision != null)
        {
            return collision.Id == currentId;
        }

        return true;
    }

    public bool IsNotGameFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation)))
        {
            throw new InvalidOperationException("Game location not set.");
        }

        return !SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation).Equals(path, StringComparison.OrdinalIgnoreCase);
    }

    public bool ValidateAlias(string alias, Guid currentId)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return false;
        }

        var collision = UserFolders
            .FirstOrDefault(x => x.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase));

        if (currentId == Guid.Empty)
        {
            return collision == null;
        }

        if (collision != null)
        {
            return collision.Id == currentId;
        }

        return true;
    }

    public SimCity4.ModManager.App.Model.UserFolder GetUserFolderDataByPath(string path)
    {
        return userFolderController.LoadUserFolder(path);
    }

    private void UpdateIsStartupFolder(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        if (userFolder.IsMainFolder)
        {
            userFolder.IsStartupFolder = false;
        }

        if (!userFolder.IsStartupFolder)
        {
            return;
        }

        foreach (var folder in UserFolders.Where(x => x.IsStartupFolder && x.Id != userFolder.Id))
        {
            folder.IsStartupFolder = false;
        }
    }
}