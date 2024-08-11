using System;
using System.IO;

namespace SimCity4.ModManager.App.Application.Utilities;

public static class FileSystemLocationsUtil
{
    public static string LocalApplicationDirectory
    {
        get
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Irradiated Games", "SimCity 4 Buddy");
        }
    }

    public static string LogFilesDirectory
    {
        get
        {
            return Path.Combine(LocalApplicationDirectory, "Logs");
        }
    }

    public static string LocalApplicationDataDirectory
    {
        get
        {
            return Path.Combine(LocalApplicationDirectory, "DataStorage");
        }
    }
}