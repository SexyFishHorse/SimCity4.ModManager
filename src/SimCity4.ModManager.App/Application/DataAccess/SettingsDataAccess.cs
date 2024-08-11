using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;

namespace SimCity4.ModManager.App.Application.DataAccess;

public class SettingsDataAccess(string storageLocation, string filename)
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public IDictionary<string, object> Settings { get; private set; } = new Dictionary<string, object>();

    public string DataLocation { get; set; } = Path.Combine(storageLocation, "Configuration", filename);

    public void LoadSettingsFromDisc()
    {
        if (!File.Exists(DataLocation))
        {
            return;
        }

        using (var reader = new StreamReader(DataLocation))
        {
            var json = reader.ReadToEnd();

            Settings = JsonConvert.DeserializeObject<IDictionary<string, object>>(json);
        }
    }

    public void StoreSettingsToDisc()
    {
        if (!Settings.Any())
        {
            Log.Info($"Empty collection for {DataLocation}, skipping storage.");
            return;
        }

        var fileInfo = new FileInfo(DataLocation);

        if (fileInfo.DirectoryName == null)
        {
            throw new DirectoryNotFoundException($"The location string {DataLocation} does not contain a directory name.");
        }

        Directory.CreateDirectory(fileInfo.DirectoryName);

        using (var writer = new StreamWriter(DataLocation))
        {
            writer.Write(JsonConvert.SerializeObject(Settings));
        }
    }

    public void SetSetting(string key, object value)
    {
        if (!Settings.Any())
        {
            LoadSettingsFromDisc();
        }

        Settings[key] = value;

        StoreSettingsToDisc();
    }

    public bool HasSetting(string key)
    {
        if (!Settings.Any())
        {
            LoadSettingsFromDisc();
        }

        return Settings.ContainsKey(key);
    }
}