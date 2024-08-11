using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json.Linq;

namespace SimCity4.ModManager.App.UserFolders.DataAccess;

public class UserFoldersDataAccess(SimCity4.ModManager.App.Utils.IJsonFileWriter writer) : IUserFoldersDataAccess
{
    public const string Filename = "UserFolders.json";

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public ICollection<SimCity4.ModManager.App.Model.UserFolder> LoadUserFolders()
    {
        Log.Info("Loading all user folders.");

        var path = Path.Combine(SimCity4.ModManager.App.Application.Utilities.FileSystemLocationsUtil.LocalApplicationDataDirectory, Filename);
        var fileInfo = new FileInfo(path);

        var userFolders = new Collection<SimCity4.ModManager.App.Model.UserFolder>();

        if (!fileInfo.Exists)
        {
            return userFolders;
        }

        using (var reader = new StreamReader(path))
        {
            var json = reader.ReadToEnd();
            dynamic userFoldersJson = JArray.Parse(json);

            foreach (var userFolderJson in userFoldersJson)
            {
                var userFolder = new SimCity4.ModManager.App.Model.UserFolder((Guid)userFolderJson.Id)
                {
                    Alias = userFolderJson.Alias,
                    FolderPath = userFolderJson.FolderPath,
                    IsMainFolder = userFolderJson.IsMainFolder,
                    IsStartupFolder = userFolderJson.IsStartupFolder
                };

                userFolders.Add(userFolder);
            }

            return userFolders;
        }
    }

    public void SaveUserFolders(IEnumerable<SimCity4.ModManager.App.Model.UserFolder> userFolders)
    {
        Log.Info("Save all user folders.");
        var fileInfo = new FileInfo(Path.Combine(SimCity4.ModManager.App.Application.Utilities.FileSystemLocationsUtil.LocalApplicationDataDirectory, Filename));

        writer.WriteToFile(fileInfo, userFolders);
    }
}