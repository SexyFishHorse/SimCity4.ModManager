using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using log4net;

namespace SimCity4.ModManager.App.Application.Control;

using Timer = System.Threading.Timer;

public class GameLauncher(ProcessStartInfo gameProcessStartInfo, int autoSaveWaitTime)
    : IDisposable
{
    private const int MillisecondsPrMinute = 60000;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private Process gameProcess;

    private Timer timer;

    public void Dispose()
    {
        timer.Dispose();
    }

    public void Start()
    {
        Log.Info($"Starting game with the following arguments: {gameProcessStartInfo.Arguments}");

        gameProcess = Process.Start(gameProcessStartInfo);

        if (gameProcess == null)
        {
            Log.Error("Unable to launch game.");

            return;
        }

        gameProcess.Exited += (sender, args) => Dispose();
        var handle = gameProcess.Handle;

        if (!SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.EnableAutoSave))
        {
            return;
        }

        Log.Info($"Autosave is enabled. Attempting to save the game every {autoSaveWaitTime} minutes.");

        timer = new Timer(
            SendSaveCommand,
            handle,
            autoSaveWaitTime * MillisecondsPrMinute,
            autoSaveWaitTime * MillisecondsPrMinute);
    }

    [DllImport("User32.dll")]
    private static extern int SetForegroundWindow(IntPtr handle);

    private void SendSaveCommand(object state)
    {
        Log.Info("Sending save signal to the game.");
        SetForegroundWindow((IntPtr)state);
        SendKeys.SendWait("^%(s)");
    }
}