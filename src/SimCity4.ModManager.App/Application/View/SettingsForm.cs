using System;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using log4net;

namespace SimCity4.ModManager.App.Application.View;

using SimCity4.ModManager.App.Properties;
using SimCity4.ModManager.App.Resources;
using ColorDepth = SimCity4.ModManager.App.Application.Models.ColorDepth;

public partial class SettingsForm : Form
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly Version MinOsVersion = new(6, 2);

    private readonly SimCity4.ModManager.App.Application.Control.ISettingsController settingsController;

    public SettingsForm(SimCity4.ModManager.App.UserFolders.Control.IUserFoldersController userFoldersController)
    {
        InitializeComponent();

        settingsController = new SimCity4.ModManager.App.Application.Control.SettingsController(userFoldersController);

        if (Environment.OSVersion.Version < MinOsVersion)
        {
            scanButton.Text = string.Empty;
            scanButton.Image = Resources.IconZoom;
        }

        if (!SimCity4.ModManager.App.Configuration.Settings.HasSetting(SimCity4.ModManager.App.Configuration.Settings.Keys.QuarantinedFiles))
        {
            SimCity4.ModManager.App.Configuration.Settings.SetAndSave(
                SimCity4.ModManager.App.Configuration.Settings.Keys.QuarantinedFiles,
                Path.Combine(SimCity4.ModManager.App.Configuration.Settings.GetDefaultStorageLocation(), "QuarantinedFiles"));
        }
    }

    private void AutoSaveIntervalTrackBarScroll(object sender, EventArgs e)
    {
        var interval = autoSaveIntervalTrackBar.Value;

        UpdateAutoSaveLabel(interval);
    }

    private void BrowseButtonClick(object sender, EventArgs e)
    {
        var result = gameLocationDialog.ShowDialog(this);

        if (result == DialogResult.Cancel)
        {
            return;
        }

        var path = gameLocationDialog.SelectedPath;

        if (settingsController.ValidateGameLocationPath(path))
        {
            Log.Info($"Browsed to valid game location: {path}");
            gameLocationTextBox.Text = path;
        }

        UpdateLanguageComboBox();
    }

    private void BrowseQuarantinedButtonClick(object sender, EventArgs e)
    {
        var result = storeLocationDialog.ShowDialog(this);
        if (result == DialogResult.OK)
        {
            quarantinedFilesLocationTextBox.Text = storeLocationDialog.SelectedPath;
        }
    }

    private void CloseButtonClick(object sender, EventArgs e)
    {
        Log.Info("Closing settings form");
        Hide();
    }

    private void DisableAudioCheckBoxCheckedChanged(object sender, EventArgs e)
    {
        disableMusicCheckBox.Enabled = !disableAudioCheckBox.Checked;
        disableSoundsCheckBox.Enabled = !disableAudioCheckBox.Checked;
    }

    private void EnableAutoSaveButtonCheckedChanged(object sender, EventArgs e)
    {
        autoSaveIntervalTrackBar.Enabled = enableAutoSaveCheckBox.Checked;

        AutoSaveIntervalTrackBarScroll(sender, e);
    }

    private void GameLocationTextBoxTextChanged(object sender, EventArgs e)
    {
        if (gameLocationTextBox.Text.Length < 1)
        {
            gameLocationTextBox.Text = LocalizationStrings.SelectGameLocation;
        }

        gameLocationTextBox.ForeColor = gameLocationTextBox.Text.Equals(
            LocalizationStrings.SelectGameLocation,
            StringComparison.OrdinalIgnoreCase)
            ? Color.Gray
            : Color.Black;

        UpdateLanguageComboBox();
    }

    private void OkButtonClick(object sender, EventArgs e)
    {
        if (!settingsController.ValidateGameLocationPath(gameLocationTextBox.Text))
        {
            Log.Info("OK pressed, no valid game folder set.");
            MessageBox.Show(
                this,
                LocalizationStrings.InvalidGameLocationFolder,
                LocalizationStrings.GameNotFound,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        SimCity4.ModManager.App.Configuration.Settings.SetAndSave(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation, gameLocationTextBox.Text);

        if (!ValidateResolution())
        {
            Log.Info($"Invalid resolution: \"{resolutionComboBox.Text.Trim()}\"");
            MessageBox.Show(
                this,
                LocalizationStrings.ResolutionMustBeInTheFormatNumberXNumber,
                LocalizationStrings.ValidationError,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);

            return;
        }

        if (backgroundImageListView.SelectedIndices.Count > 0)
        {
            SimCity4.ModManager.App.Configuration.Settings.SetAndSave(SimCity4.ModManager.App.Configuration.Settings.Keys.Wallpaper, backgroundImageListView.SelectedIndices[0] + 1);
        }

        if (renderModeComboBox.SelectedIndex > 0)
        {
            var renderMode = ((SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.RenderMode>)renderModeComboBox.SelectedItem).Value;
            SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.RenderMode, renderMode.ToString());
        }
        else
        {
            SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.RenderMode, string.Empty);
        }

        var regex = new Regex(@"\d+x\d+");
        var resolution = resolutionComboBox.Text.Trim();
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(
            SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.Resolution,
            regex.IsMatch(resolution) ? resolution : string.Empty);

        var colorDepth = ((SimCity4.ModManager.App.UI.Elements.ComboBoxItem<ColorDepth>)colourDepthComboBox.SelectedItem).Value;
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(
            SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.ColourDepth32Bit,
            colorDepth == ColorDepth.Bits32);

        var cursorColour = cursorColourComboBox.SelectedIndex > 0 ?
            ((SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CursorColorDepth>)cursorColourComboBox.SelectedItem).Value.ToString()
            : string.Empty;
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(
            SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CursorColourDepth,
            cursorColour);

        var numCpus = Convert.ToInt32(cpuCountComboBox.Text.Trim());
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CpuCount, numCpus);

        var cpuPriority = cpuPriorityComboBox.SelectedIndex > 0
            ? ((SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CpuPriority>)
                cpuPriorityComboBox.SelectedItem).Value.ToString()
            : string.Empty;

        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CpuPriority, cpuPriority);

        settingsController.CheckMainFolder();

        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.EnableAutoSave, enableAutoSaveCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.AutoSaveWaitTime, autoSaveIntervalTrackBar.Value);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableAudio, disableAudioCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableMusic, disableMusicCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableSounds, disableSoundsCheckBox.Checked);

        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.EnableCustomResolution, customResolutionCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.WindowMode, windowModeCheckBox.Checked);

        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.SkipIntro, skipIntroCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.PauseWhenMinimized, pauseMinimizedCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(
            SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableExceptionHandling,
            disableExceptionHandlingCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(
            SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableBackgroundLoader,
            disableBackgroundLoaderCheckBox.Checked);

        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.Language, languageComboBox.SelectedItem);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.IgnoreMissingModels, ignoreMissingModelsCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableIme, disableIMECheckBox.Checked);
        SimCity4.ModManager.App.Configuration.LauncherSettings.SetAndSave(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.WriteLog, writeLogCheckBox.Checked);

        SimCity4.ModManager.App.Configuration.Settings.SetAndSave(
            SimCity4.ModManager.App.Configuration.Settings.Keys.AllowCheckForMissingDependencies,
            allowCheckMissingDependenciesCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.Settings.SetAndSave(
            SimCity4.ModManager.App.Configuration.Settings.Keys.AskForAdditionalInformationAfterInstallation,
            AskForAdditionalInfoAfterInstallCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.Settings.SetAndSave(
            SimCity4.ModManager.App.Configuration.Settings.Keys.AskToRemoveNonPluginFilesAfterInstallation,
            RemoveNonPluginFilesAfterInstallCheckBox.Checked);
        SimCity4.ModManager.App.Configuration.Settings.SetAndSave(
            SimCity4.ModManager.App.Configuration.Settings.Keys.AutoRunExecutablesDuringInstallation,
            AutoRunInstallerExecutablesCheckBox.Checked);

        SimCity4.ModManager.App.Configuration.Settings.SetAndSave(SimCity4.ModManager.App.Configuration.Settings.Keys.ApiBaseUrl, apiBaseUrlTextBox.Text.Trim());
        SimCity4.ModManager.App.Configuration.Settings.SetAndSave(
            SimCity4.ModManager.App.Configuration.Settings.Keys.FetchInformationFromRemoteServer,
            fetchInformationFromRemoteCheckbox.Checked);
        SimCity4.ModManager.App.Configuration.Settings.SetAndSave(SimCity4.ModManager.App.Configuration.Settings.Keys.DetectPlugins, detectPluginsCheckBox.Checked);

        Close();
    }

    private void ScanButtonClick(object sender, EventArgs e)
    {
        var gameLocation = settingsController.SearchForGameLocation();

        if (string.IsNullOrWhiteSpace(gameLocation))
        {
            Log.Info("Could not find game location using the scanner");

            MessageBox.Show(
                this,
                LocalizationStrings.UnableToLocateTheGameUseTheBrowseOptionInstead,
                LocalizationStrings.GameNotFound,
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
        }
        else
        {
            Log.Info("Game found using scanner");

            gameLocationTextBox.Text = gameLocation;
            UpdateLanguageComboBox();
        }
    }

    private void SettingsFormFormClosing(object sender, FormClosingEventArgs e)
    {
        if (settingsController.ValidateGameLocationPath(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation)))
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            LocalizationStrings.YouMustSettheGameLocationBeforeYouCanUseThisApplication,
            LocalizationStrings.GameFolderNotSet,
            MessageBoxButtons.RetryCancel,
            MessageBoxIcon.Exclamation,
            MessageBoxDefaultButton.Button1);

        if (result != DialogResult.Retry)
        {
            return;
        }

        Log.Info("Abort form close");
        e.Cancel = true;
    }

    private void SettingsFormLoad(object sender, EventArgs e)
    {
        gameLocationTextBox.Text = SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation);

        if (string.IsNullOrWhiteSpace(gameLocationTextBox.Text))
        {
            gameLocationTextBox.Text = LocalizationStrings.SelectGameLocation;
            gameLocationTextBox.ForeColor = Color.Gray;
        }

        gameLocationTextBox.ForeColor = gameLocationTextBox.Text.Equals(
            LocalizationStrings.SelectGameLocation,
            StringComparison.OrdinalIgnoreCase)
            ? Color.Gray
            : Color.Black;

        enableAutoSaveCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.EnableAutoSave);
        autoSaveIntervalTrackBar.Enabled = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.EnableAutoSave);
        UpdateAutoSaveLabel(SimCity4.ModManager.App.Configuration.LauncherSettings.GetInt(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.AutoSaveWaitTime));

        disableAudioCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableAudio);
        disableMusicCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableMusic);
        disableSoundsCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableSounds);

        customResolutionCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.EnableCustomResolution);
        windowModeCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.WindowMode);

        UpdateResolutionComboBox();
        UpdateRenderModeComboBox();
        UpdateColourDepthComboBox();
        UpdateCursorColourComboBox();

        UpdateCpuCountComboBox();
        UpdateCpuPriorityComboBox();

        skipIntroCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.SkipIntro);
        pauseMinimizedCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.PauseWhenMinimized);
        disableExceptionHandlingCheckBox.Checked =
            SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableExceptionHandling);
        disableBackgroundLoaderCheckBox.Checked =
            SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableBackgroundLoader);

        UpdateLanguageComboBox();

        ignoreMissingModelsCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.IgnoreMissingModels);
        disableIMECheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.DisableIme);
        writeLogCheckBox.Checked = SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.WriteLog);

        UpdateBackgroundsListView();

        quarantinedFilesLocationTextBox.Text = SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.QuarantinedFiles);

        allowCheckMissingDependenciesCheckBox.Checked =
            SimCity4.ModManager.App.Configuration.Settings.Get<bool>(SimCity4.ModManager.App.Configuration.Settings.Keys.AllowCheckForMissingDependencies);
        AskForAdditionalInfoAfterInstallCheckBox.Checked =
            SimCity4.ModManager.App.Configuration.Settings.Get<bool>(SimCity4.ModManager.App.Configuration.Settings.Keys.AskForAdditionalInformationAfterInstallation);
        RemoveNonPluginFilesAfterInstallCheckBox.Checked =
            SimCity4.ModManager.App.Configuration.Settings.Get<bool>(SimCity4.ModManager.App.Configuration.Settings.Keys.AskToRemoveNonPluginFilesAfterInstallation);
        AutoRunInstallerExecutablesCheckBox.Checked =
            SimCity4.ModManager.App.Configuration.Settings.Get<bool>(SimCity4.ModManager.App.Configuration.Settings.Keys.AutoRunExecutablesDuringInstallation);

        apiBaseUrlTextBox.Text = SimCity4.ModManager.App.Configuration.Settings.Get(
            SimCity4.ModManager.App.Configuration.Settings.Keys.ApiBaseUrl,
            ConfigurationManager.AppSettings["ApiBaseUrl"]);
        detectPluginsCheckBox.Checked = SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.DetectPlugins, true);
        fetchInformationFromRemoteCheckbox.Checked =
            SimCity4.ModManager.App.Configuration.Settings.Get<bool>(SimCity4.ModManager.App.Configuration.Settings.Keys.FetchInformationFromRemoteServer);
    }

    private void UpdateAutoSaveLabel(int interval)
    {
        autoSaveIntervalLabel.Text = string.Format(LocalizationStrings.NumMinutes, interval);
        shortAutosaveIntervalsLabel.Visible = interval < 10;
    }

    private void UpdateBackgroundsListView()
    {
        var wallpapers = settingsController.GetWallpapers();

        backgroundImageListView.BeginUpdate();
        var imageList = new ImageList { ImageSize = new Size(65, 65) };
        backgroundImageListView.LargeImageList = imageList;

        for (var index = 0; index < wallpapers.Count; index++)
        {
            var wallpaper = wallpapers[index];
            var item = new ListViewItem((index + 1).ToString(CultureInfo.InvariantCulture));
            imageList.Images.Add(wallpaper);
            item.ImageIndex = index;

            backgroundImageListView.Items.Add(item);
        }

        backgroundImageListView.EndUpdate();
    }

    private void UpdateColourDepthComboBox()
    {
        colourDepthComboBox.BeginUpdate();
        colourDepthComboBox.Items.Clear();
        colourDepthComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<ColorDepth>(
                LocalizationStrings.Bits16,
                ColorDepth.Bits16));
        colourDepthComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<ColorDepth>(
                LocalizationStrings.Bits32,
                ColorDepth.Bits32));

        colourDepthComboBox.SelectedIndex =
            SimCity4.ModManager.App.Configuration.LauncherSettings.Get<bool>(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.ColourDepth32Bit) ? 1 : 0;
        colourDepthComboBox.EndUpdate();
    }

    private void UpdateCpuCountComboBox()
    {
        cpuCountComboBox.BeginUpdate();
        cpuCountComboBox.Items.Clear();
        cpuCountComboBox.Items.Add(LocalizationStrings.Ignore);
        for (var i = 1; i <= Environment.ProcessorCount; i++)
        {
            cpuCountComboBox.Items.Add(i);
        }

        cpuCountComboBox.SelectedIndex = SimCity4.ModManager.App.Configuration.LauncherSettings.GetInt(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CpuCount);
        cpuCountComboBox.EndUpdate();
    }

    private void UpdateCpuPriorityComboBox()
    {
        cpuPriorityComboBox.BeginUpdate();
        cpuPriorityComboBox.Items.Clear();
        cpuPriorityComboBox.Items.Add(LocalizationStrings.Ignore);
        cpuPriorityComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CpuPriority>(
                LocalizationStrings.Low,
                SimCity4.ModManager.App.Application.Models.CpuPriority.Low));
        cpuPriorityComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CpuPriority>(
                LocalizationStrings.Medium,
                SimCity4.ModManager.App.Application.Models.CpuPriority.Medium));
        cpuPriorityComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CpuPriority>(
                LocalizationStrings.High,
                SimCity4.ModManager.App.Application.Models.CpuPriority.High));

        Enum.TryParse(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CpuPriority), true, out SimCity4.ModManager.App.Application.Models.CpuPriority selectedPriority);
        switch (selectedPriority)
        {
            case SimCity4.ModManager.App.Application.Models.CpuPriority.Low:
                cpuPriorityComboBox.SelectedIndex = 1;
                break;
            case SimCity4.ModManager.App.Application.Models.CpuPriority.Medium:
                cpuPriorityComboBox.SelectedIndex = 2;
                break;
            case SimCity4.ModManager.App.Application.Models.CpuPriority.High:
                cpuPriorityComboBox.SelectedIndex = 3;
                break;
            default:
                cpuPriorityComboBox.SelectedIndex = 0;
                break;
        }

        cpuPriorityComboBox.EndUpdate();
    }

    private void UpdateCursorColourComboBox()
    {
        cursorColourComboBox.BeginUpdate();
        cursorColourComboBox.Items.Clear();
        cursorColourComboBox.Items.Add(LocalizationStrings.Ignore);
        cursorColourComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CursorColorDepth>(
                LocalizationStrings.Disabled,
                SimCity4.ModManager.App.Application.Models.CursorColorDepth.Disabled));
        cursorColourComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CursorColorDepth>(
                LocalizationStrings.SystemCursors,
                SimCity4.ModManager.App.Application.Models.CursorColorDepth.SystemCursors));
        cursorColourComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CursorColorDepth>(
                LocalizationStrings.BlackAndWhite,
                SimCity4.ModManager.App.Application.Models.CursorColorDepth.BlackAndWhite));
        cursorColourComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CursorColorDepth>(
                LocalizationStrings.Colors16,
                SimCity4.ModManager.App.Application.Models.CursorColorDepth.Colors16));
        cursorColourComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CursorColorDepth>(
                LocalizationStrings.Colors256,
                SimCity4.ModManager.App.Application.Models.CursorColorDepth.Colors256));
        cursorColourComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.CursorColorDepth>(
                LocalizationStrings.FullColors,
                SimCity4.ModManager.App.Application.Models.CursorColorDepth.FullColors));

        Enum.TryParse(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.CursorColourDepth), true, out SimCity4.ModManager.App.Application.Models.CursorColorDepth selectedCursor);

        switch (selectedCursor)
        {
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.Disabled:
                cursorColourComboBox.SelectedIndex = 1;
                break;
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.SystemCursors:
                cursorColourComboBox.SelectedIndex = 2;
                break;
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.BlackAndWhite:
                cursorColourComboBox.SelectedIndex = 3;
                break;
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.Colors16:
                cursorColourComboBox.SelectedIndex = 4;
                break;
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.Colors256:
                cursorColourComboBox.SelectedIndex = 5;
                break;
            case SimCity4.ModManager.App.Application.Models.CursorColorDepth.FullColors:
                cursorColourComboBox.SelectedIndex = 6;
                break;
            default:
                cursorColourComboBox.SelectedIndex = 0;
                break;
        }

        cursorColourComboBox.EndUpdate();
    }

    private void UpdateLanguageComboBox()
    {
        if (!settingsController.ValidateGameLocationPath(SimCity4.ModManager.App.Configuration.Settings.Get(SimCity4.ModManager.App.Configuration.Settings.Keys.GameLocation)))
        {
            return;
        }

        var languages = settingsController.GetInstalledLanguages();

        languageComboBox.BeginUpdate();
        languageComboBox.Items.Clear();
        languageComboBox.Items.Add(LocalizationStrings.Ignore);
        languageComboBox.Items.AddRange(languages.Cast<object>().ToArray());

        languageComboBox.SelectedItem =
            string.IsNullOrWhiteSpace(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.Language))
                ? LocalizationStrings.Ignore
                : SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.Language);

        languageComboBox.EndUpdate();
    }

    private void UpdateRenderModeComboBox()
    {
        renderModeComboBox.BeginUpdate();
        renderModeComboBox.Items.Clear();
        renderModeComboBox.Items.Add(LocalizationStrings.Ignore);
        renderModeComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.RenderMode>(
                LocalizationStrings.DirectX,
                SimCity4.ModManager.App.Application.Models.RenderMode.DirectX));
        renderModeComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.RenderMode>(
                LocalizationStrings.OpenGL,
                SimCity4.ModManager.App.Application.Models.RenderMode.OpenGl));
        renderModeComboBox.Items.Add(
            new SimCity4.ModManager.App.UI.Elements.ComboBoxItem<SimCity4.ModManager.App.Application.Models.RenderMode>(
                LocalizationStrings.Software,
                SimCity4.ModManager.App.Application.Models.RenderMode.Software));

        Enum.TryParse(SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.RenderMode), true, out SimCity4.ModManager.App.Application.Models.RenderMode selectedRenderMode);

        switch (selectedRenderMode)
        {
            case SimCity4.ModManager.App.Application.Models.RenderMode.DirectX:
                renderModeComboBox.SelectedIndex = 1;

                break;
            case SimCity4.ModManager.App.Application.Models.RenderMode.OpenGl:
                renderModeComboBox.SelectedIndex = 2;

                break;
            case SimCity4.ModManager.App.Application.Models.RenderMode.Software:
                renderModeComboBox.SelectedIndex = 3;

                break;
            default:
                renderModeComboBox.SelectedIndex = 0;

                break;
        }

        renderModeComboBox.EndUpdate();
    }

    private void UpdateResolutionComboBox()
    {
        resolutionComboBox.Text = SimCity4.ModManager.App.Configuration.LauncherSettings.Get(SimCity4.ModManager.App.Configuration.LauncherSettings.Keys.Resolution);
    }

    private bool ValidateResolution()
    {
        var regEx = new Regex(@"\d+x\d+");
        var text = resolutionComboBox.Text.Trim();

        return regEx.IsMatch(text)
               || string.IsNullOrWhiteSpace(text)
               || text.Equals(LocalizationStrings.Ignore, StringComparison.OrdinalIgnoreCase);
    }
}