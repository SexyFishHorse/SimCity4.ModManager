using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.Win32;

namespace SimCity4.ModManager.App.Application.Control;

using SimCity4.ModManager.App.Properties;
using SimCity4.ModManager.App.Resources;

public class SettingsController(SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController) : ISettingsController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly string[] regKeys =
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

    public bool ValidateGameLocationPath(string path)
    {
        Log.Info($"Validating game path: {path}");

        if (string.IsNullOrWhiteSpace(path) || path.Equals(LocalizationStrings.SelectGameLocation))
        {
            Log.Error("Game location is empty");
            return false;
        }

        var exists = File.Exists(Path.Combine(path, @"Apps\SimCity 4.exe"));

        Log.Info("Path is " + (exists ? "valid" : "invalid"));
        return exists;
    }

    public void CheckMainFolder()
    {
        Log.Info("Checking main folder");

        var folder = userFoldersController.GetMainUserFolder();
        if (folder == null)
        {
            throw new InvalidOperationException("Main plugin folder has been deleted from the database.");
        }

        folder.FolderPath = SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation);
        folder.Alias = LocalizationStrings.GameUserFolderName;
        userFoldersController.Update(folder);
    }

    public string SearchForGameLocation()
    {
        Log.Info("Searching for game location");

        foreach (var regKey in regKeys)
        {
            Log.Info($"Checking key: {regKey}");
            using (var key = Registry.LocalMachine.OpenSubKey(regKey))
            {
                if (key == null)
                {
                    continue;
                }

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using (var subKey = key.OpenSubKey(subKeyName))
                    {
                        if (subKey == null || string.IsNullOrWhiteSpace((string)subKey.GetValue("DisplayName")))
                        {
                            continue;
                        }

                        var name = (string)subKey.GetValue("DisplayName");
                        var path = (string)subKey.GetValue("InstallLocation");

                        if (!name.StartsWith("SimCity 4") || string.IsNullOrWhiteSpace(path) || !ValidateGameLocationPath(path))
                        {
                            continue;
                        }

                        Log.Info($"Found a match. Name: {name}, Path: {path}");

                        return path;
                    }
                }
            }
        }

        Log.Error("Unable to auto locate game.");
        return string.Empty;
    }

    public IEnumerable<string> GetInstalledLanguages()
    {
        var dirs = Directory.EnumerateDirectories(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation), "*", SearchOption.TopDirectoryOnly);

        var languages =
            dirs.Select(
                    dir => new { dir, files = Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly) })
                .Where(
                    @t =>
                        @t.files.Any(file => file.EndsWith("SimCityLocale.dat", StringComparison.OrdinalIgnoreCase)))
                .Select(@t => new DirectoryInfo(@t.dir).Name)
                .ToList();

        return languages;
    }

    public IList<Bitmap> GetWallpapers()
    {
        return new List<Bitmap>
        {
            Resources.Wallpaper1,
            Resources.Wallpaper2,
            Resources.Wallpaper3,
            Resources.Wallpaper4,
            Resources.Wallpaper5,
            Resources.Wallpaper6,
            Resources.Wallpaper7,
            Resources.Wallpaper8,
            Resources.Wallpaper9,
            Resources.Wallpaper10,
            Resources.Wallpaper11,
            Resources.Wallpaper12,
            Resources.Wallpaper13
        };
    }
}