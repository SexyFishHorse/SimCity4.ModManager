using System.Globalization;
using System.Windows.Forms;

namespace SimCity4.ModManager.App.View.Elements;

public class RemovalFileTypeCandidateListViewItem : ListViewItem
{
    public RemovalFileTypeCandidateListViewItem(SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo candidateInfo)
    {
        CandidateInfo = candidateInfo;

        Text = candidateInfo.FileTypeInfo.DescriptiveName;

        SubItems.Add(new ListViewSubItem(this, candidateInfo.FileTypeInfo.Extension));
        SubItems.Add(new ListViewSubItem(this, candidateInfo.NumberOfEntities.ToString(CultureInfo.InvariantCulture)));
        SubItems.Add(new ListViewSubItem(this, candidateInfo.FileTypeInfo.Description));
    }

    public SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo CandidateInfo { get; set; }
}