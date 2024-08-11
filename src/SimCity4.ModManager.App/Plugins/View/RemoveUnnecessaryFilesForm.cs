using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace SimCity4.ModManager.App.Plugins.View;

public partial class RemoveUnnecessaryFilesForm : Form
{
    public RemoveUnnecessaryFilesForm()
    {
        InitializeComponent();
    }

    public Collection<SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo> ToBeRemoved { get; set; }

    public void SetCandidateInfos(IEnumerable<SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo> candidateInfos)
    {
        fileTypesListView.BeginUpdate();
        fileTypesListView.Items.Clear();

        foreach (var candidateInfo in candidateInfos)
        {
            fileTypesListView.Items.Add(
                new SimCity4.ModManager.App.View.Elements.RemovalFileTypeCandidateListViewItem(candidateInfo));
        }

        fileTypesListView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
        fileTypesListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
        fileTypesListView.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.HeaderSize);
        fileTypesListView.AutoResizeColumn(3, ColumnHeaderAutoResizeStyle.ColumnContent);
        fileTypesListView.EndUpdate();
    }

    private void RemoveSelectedButtonClick(object sender, System.EventArgs e)
    {
        var checkedItems = fileTypesListView.CheckedItems;

        var toBeRemoved = new Collection<SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo>();

        foreach (SimCity4.ModManager.App.View.Elements.RemovalFileTypeCandidateListViewItem checkedItem in checkedItems)
        {
            toBeRemoved.Add(checkedItem.CandidateInfo);
        }

        ToBeRemoved = toBeRemoved;
    }
}