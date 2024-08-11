using System.Windows.Forms;

namespace SimCity4.ModManager.App.View.Elements;

public class ListViewItemWithObjectValue<T>(string text, T value) : ListViewItem(text)
{
    public T Value { get; set; } = value;
}