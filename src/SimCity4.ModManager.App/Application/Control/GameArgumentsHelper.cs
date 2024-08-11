using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace SimCity4.ModManager.App.Application.Control;

public class GameArgumentsHelper
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly Regex ResolutionRegEx = new(@"\d+x\d+");

    public string GetArgumentString(SimCity4.ModManager.App.Model.UserFolder selectedUserFolder)
    {
        var arguments = new List<string>();
        arguments.AddRange(GetAudioArguments());
        arguments.AddRange(GetVideoArguments());
        arguments.AddRange(GetPerformanceArguments());
        arguments.AddRange(GetOtherArguments());

        if (selectedUserFolder != null)
        {
            arguments.Add($"-userDir:\"{selectedUserFolder.FolderPath}\\\"");
        }

        return string.Join(" ", arguments.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
    }

    private static IEnumerable<string> GetAudioArguments()
    {
        var output = new Collection<string>
        {
            $"-audio:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableAudio) ? "off" : "on")}",
            $"-music:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableMusic) ? "off" : "on")}",
            $"-sounds:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableSounds) ? "off" : "on")}"
        };

        return output;
    }

    private static IEnumerable<string> GetOtherArguments()
    {
        var output = new Collection<string>
        {
            $"-l:{SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.Language)}",
            $"-ignoreMissingModelDataBugs:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.IgnoreMissingModels) ? "on" : "off")}",
            $"-ime:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableIme) ? "disabled" : "enabled")}",
            $"-writeLog:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.WriteLog) ? "enabled" : "disabled")}"
        };

        return output;
    }

    private static IEnumerable<string> GetPerformanceArguments()
    {
        var output = new Collection<string>
        {
            $"-intro:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.SkipIntro) ? "off" : "on")}",
            $"-exceptionHandling:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableBackgroundLoader) ? "off" : "on")}",
            $"-backgroundLoader:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableBackgroundLoader) ? "off" : "on")}"
        };

        if (SimCity4.ModManager.App.Configuration.LauncherSettings.GetInt(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CpuCount) > 0)
        {
            output.Add($"-cpuCount:{SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CpuCount)}");
        }

        if (!string.IsNullOrWhiteSpace(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CpuPriority)))
        {
            if (Enum.TryParse(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CpuPriority), true, out SimCity4.ModManager.App.Application.Models.CpuPriority priority))
            {
                output.Add(GetStringForCpuPriority(priority));
            }
            else
            {
                Log.Warn(
                    $"Unknown CPU priority: \"{SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CpuPriority)}\", skipping argument.");
            }
        }

        if (SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.PauseWhenMinimized))
        {
            output.Add("-gp");
        }

        return output;
    }

    private static string GetStringForCpuPriority(SimCity4.ModManager.App.Application.Models.CpuPriority priority)
    {
        var builder = new StringBuilder("-cpuPriority:");

        switch (priority)
        {
            case SimCity4.ModManager.App.Application.Models.CpuPriority.Low:
                builder.Append("low");

                break;
            case SimCity4.ModManager.App.Application.Models.CpuPriority.Medium:
                builder.Append("medium");

                break;
            case SimCity4.ModManager.App.Application.Models.CpuPriority.High:
                builder.Append("high");

                break;
        }

        return builder.ToString();
    }

    private static string GetStringForCursors(SimCity4.ModManager.App.Application.Models.CursorColorDepth cursorColorDepth)
    {
        var builder = new StringBuilder("-cursors:");

        switch (cursorColorDepth)
        {
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.Disabled:
                builder.Append("disabled");

                break;
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.BlackAndWhite:
                builder.Append("bw");

                break;
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.Colors16:
                builder.Append("color16");

                break;
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.Colors256:
                builder.Append("color256");

                break;
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.FullColors:
                builder.Append("fullcolor");

                break;
            default:
                return string.Empty;
        }

        return builder.ToString();
    }

    private static string GetStringForRenderMode(SimCity4.ModManager.App.Application.Models.RenderMode renderMode)
    {
        var builder = new StringBuilder("-d:");
        switch (renderMode)
        {
            case SimCity4.ModManager.App.Application.Models.RenderMode.DirectX:
                builder.Append("directX");
                break;
            case SimCity4.ModManager.App.Application.Models.RenderMode.OpenGl:
                builder.Append("openGl");
                break;
            case SimCity4.ModManager.App.Application.Models.RenderMode.Software:
                builder.Append("software");
                break;
        }

        return builder.ToString();
    }

    private static string GetStringForResolution(string widthTimesHeight, bool depth32)
    {
        if (!ResolutionRegEx.IsMatch(widthTimesHeight))
        {
            throw new ArgumentException(@"Must be in the format \d+x\d+", widthTimesHeight);
        }

        return $"-r{widthTimesHeight}x{(depth32 ? "32" : "16")}";
    }

    private static IEnumerable<string> GetVideoArguments()
    {
        var output = new Collection<string>
        {
            $"-customResolution:{(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.EnableCustomResolution) ? "enabled" : "disabled")}"
        };

        if (!string.IsNullOrWhiteSpace(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.Resolution)))
        {
            output.Add(
                GetStringForResolution(
                    SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.Resolution),
                    SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.ColourDepth32Bit)));
        }

        if (!string.IsNullOrWhiteSpace(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CursorColourDepth)))
        {
            if (Enum.TryParse(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CursorColourDepth), true, out SimCity4.ModManager.App.Application.Models.CursorColorDepth cursorColorDepth))
            {
                output.Add(
                    $"-customCursors:{(cursorColorDepth == SimCity4.ModManager.App.Application.Models.CursorColorDepth.SystemCursors ? "enabled" : "disabled")}");
                output.Add(GetStringForCursors(cursorColorDepth));
            }
        }

        if (Enum.TryParse(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.RenderMode), out SimCity4.ModManager.App.Application.Models.RenderMode renderMode))
        {
            output.Add(GetStringForRenderMode(renderMode));
        }

        output.Add(SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.WindowMode) ? "-w" : "-f");

        return output;
    }
}