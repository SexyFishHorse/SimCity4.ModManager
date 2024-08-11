using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;

namespace SimCity4.ModManager.App.UserFolders.DataAccess;

public class UserFolderDataAccess(SimCity4.ModManager.App.Utils.IJsonFileWriter writer) : IUserFolderDataAccess
{
    public const string Filename = "UserFolder.json";

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public SimCity4.ModManager.App.Model.UserFolder LoadUserFolder(string path)
    {
        Log.Info($"Loading user folder from path \"{path}\".");

        var file = new FileInfo(Path.Combine(path, Filename));

        if (!file.Exists)
        {
            throw new FileNotFoundException($"{file.FullName} does not exist.");
        }

        using (var reader = new StreamReader(file.FullName))
        {
            var json = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<SimCity4.ModManager.App.Model.UserFolder>(json);
        }
    }

    public void SaveUserFolder(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        Log.Info($"Saving user folder \"{userFolder.Alias}\" (id: {userFolder.Id})");

        var fileInfo = new FileInfo(Path.Combine(userFolder.FolderPath, Filename));

        writer.WriteToFile(fileInfo, userFolder);
    }
}