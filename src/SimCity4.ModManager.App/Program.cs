using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Asser.Sc4Buddy.Server.Api.V1.Client;
using log4net;
using log4net.Config;
using RestSharp;

namespace SimCity4.ModManager.App;

using SimCity4.ModManager.App.Resources;

public static class Program
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    [STAThread]
    public static void Main(string[] args)
    {
        XmlConfigurator.Configure();

        Log.Info("Application starting");

        var exceptionHandling = true;
        if (args != null)
        {
            exceptionHandling = args.All(arg => arg != "-exceptionHandling:off");
            Log.Warn("Exception handling disabled by command line argument.");
        }

        try
        {
            var entities = SimCity4.ModManager.App.DataAccess.EntityFactory.Instance.Entities;
            var userFolderController = new SimCity4.ModManager.App.UserFolders.Control.UserFolderController(new SimCity4.ModManager.App.UserFolders.DataAccess.UserFolderDataAccess(new SimCity4.ModManager.App.Utils.JsonFileWriter()));
            var userFoldersController = new SimCity4.ModManager.App.UserFolders.Control.TheUserFoldersController(new SimCity4.ModManager.App.UserFolders.DataAccess.UserFoldersDataAccess(new SimCity4.ModManager.App.Utils.JsonFileWriter()), userFolderController);

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.ApplicationExit += (sender, eventArgs) => Log.Info("Application exited");

            if (string.IsNullOrWhiteSpace(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation)) || !Directory.Exists(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation)))
            {
                var settingsForm = new SimCity4.ModManager.App.Application.View.SettingsForm(userFoldersController) { StartPosition = FormStartPosition.CenterScreen };

                System.Windows.Forms.Application.Run(settingsForm);
                SetDefaultUserFolder();
            }

            if (userFoldersController.UserFolders.Any(x => x.IsMainFolder && x.FolderPath.Equals("?")))
            {
                SetDefaultUserFolder();
            }

            if (Directory.Exists(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation)))
            {
                new SimCity4.ModManager.App.Application.Control.SettingsController(userFoldersController).CheckMainFolder();
                var buddyServerClient = new BuddyServerClient(
                    new RestClient(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.ApiBaseUrl, "http://api.sc4buddy.sexyfishhorse.com")));

                System.Windows.Forms.Application.Run(
                    new SimCity4.ModManager.App.Application.View.Sc4Buddy(
                        userFoldersController,
                        new SimCity4.ModManager.App.Plugins.Control.PluginGroupController(entities),
                        new SimCity4.ModManager.App.Plugins.Services.PluginMatcher(buddyServerClient)));
            }
        }
        catch (Exception ex)
        {
            Log.Error("Uncaught error", ex);

            if (!exceptionHandling)
            {
                throw;
            }

            var showLog = MessageBox.Show(
                string.Format(LocalizationStrings.UncaughtExceptionWouldYouLikeToOpenTheLog, ex.Message),
                LocalizationStrings.UncaughtException,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);

            if (showLog == DialogResult.No)
            {
                return;
            }

            var file = $"log-{DateTime.Now.ToString("yyyy-MM-dd")}.txt";

            Process.Start(Path.Combine(SimCity4.ModManager.App.Application.Utilities.FileSystemLocationsUtil.LogFilesDirectory, file));
        }
    }

    private static void SetDefaultUserFolder()
    {
        var userFoldersController = new SimCity4.ModManager.App.UserFolders.Control.TheUserFoldersController(
            new SimCity4.ModManager.App.UserFolders.DataAccess.UserFoldersDataAccess(new SimCity4.ModManager.App.Utils.JsonFileWriter()),
            new SimCity4.ModManager.App.UserFolders.Control.UserFolderController(new SimCity4.ModManager.App.UserFolders.DataAccess.UserFolderDataAccess(new SimCity4.ModManager.App.Utils.JsonFileWriter())));

        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SimCity 4");

        if (!Directory.Exists(path) || userFoldersController.UserFolders.Any(x => x.FolderPath.Equals(path)))
        {
            return;
        }

        Log.Info($"Setting default user folder to {path}");
        userFoldersController.Add(new SimCity4.ModManager.App.Model.UserFolder { Alias = LocalizationStrings.DefaultUserFolderName, FolderPath = path });
    }
}