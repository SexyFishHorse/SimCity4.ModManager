namespace SimCity4.ModManager.App.Plugins.View;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using SimCity4.ModManager.App.Resources;
using SimCity4.ModManager.App.View.Elements;

public partial class ReadmeFilesForm : Form
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public ReadmeFilesForm(IEnumerable<FileInfo> readmeFiles)
    {
        InitializeComponent();

        foreach (var readmeFile in readmeFiles)
        {
            readmeFilesListView.Items.Add(
                new ListViewItemWithObjectValue<FileInfo>(readmeFile.Name, readmeFile));
        }
    }

    private void ReadmeFilesListViewSelectedIndexChanged(object sender, EventArgs e)
    {
        openButton.Enabled = readmeFilesListView.SelectedItems.Count > 0;
    }

    private void OpenButtonClick(object sender, EventArgs e)
    {
        var selectedValue = ((ListViewItemWithObjectValue<FileInfo>)readmeFilesListView.SelectedItems[0]).Value;
        try
        {
            Log.Info($"Opening readme file \"{selectedValue.FullName}\"");
            Process.Start(selectedValue.FullName);
        }
        catch (Win32Exception ex)
        {
            Log.Error("Error while attempting to open readme file", ex);
            MessageBox.Show(
                this,
                string.Format(LocalizationStrings.UnableToOpenThisReadmeFile, ex.Message),
                LocalizationStrings.ErrorOpeningReadmeFile,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CloseButtonClick(object sender, EventArgs e)
    {
        Close();
    }
}
