using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace SimCity4.ModManager.App.View.Helpers;

using SimCity4.ModManager.App.Resources;

public class NonPluginFilesScannerUi(string storageLocation)
{
    private readonly SimCity4.ModManager.App.Plugins.Control.NonPluginFilesScanner scanner = new(storageLocation);

    public SimCity4.ModManager.App.Model.UserFolder UserFolder { get; set; }

    public ICollection<SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo> RemovalCandidateInfos { get; set; }

    public Collection<SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo> ToBeRemoved { get; set; }

    public bool ShowDoYouWantToScanForNonPluginFiles(Form parentForm) =>
        MessageBox.Show(
            parentForm,
            LocalizationStrings.DoYouWantToScanForAndRemoveNonPluginFiles,
            LocalizationStrings.RemoveNonPluginFiles,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Asterisk,
            MessageBoxDefaultButton.Button1) == DialogResult.Yes;

    public void ScanForCandidates()
    {
        RemovalCandidateInfos = scanner.GetFilesAndFoldersToRemove(UserFolder);
    }

    public void ShowThereAreNoEntitiesToRemoveDialog(Form parentForm)
    {
        MessageBox.Show(
            parentForm,
            LocalizationStrings.ThereAreNoNonPluginFilesOrEmptyFoldersToRemove,
            LocalizationStrings.NoNonPluginFilesDetected,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    public bool ShowConfirmDialog(Form parentForm)
    {
        var dialog = new SimCity4.ModManager.App.Plugins.View.RemoveUnnecessaryFilesForm();
        dialog.SetCandidateInfos(RemovalCandidateInfos);

        var continueDeletion = dialog.ShowDialog(parentForm) == DialogResult.OK;

        if (continueDeletion)
        {
            ToBeRemoved = dialog.ToBeRemoved;
        }

        return continueDeletion;
    }

    public void ShowRemovalSummary(Form parentForm, SimCity4.ModManager.App.Model.NonPluginFileRemovalSummary removalSummary)
    {
        var message = string.Format(LocalizationStrings.NumFilesAndNumFoldersWereRemoved,
            removalSummary.NumFilesRemoved, removalSummary.NumFoldersRemoved);

        if (removalSummary.Errors.Any())
        {
            message = string.Format(LocalizationStrings.NErrorsOccuredCheckTheLogForFurtherDetails, message,
                removalSummary.Errors.Count);
        }

        MessageBox.Show(
            parentForm,
            message,
            LocalizationStrings.NonPluginFilesDeleted,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1);
    }

    public void RemoveNonPluginFilesAndShowSummary(Form form)
    {
        var removalSummary = scanner.RemoveNonPluginFiles(UserFolder, ToBeRemoved.Select(x => x.FileTypeInfo));
        ShowRemovalSummary(form, removalSummary);
    }
}