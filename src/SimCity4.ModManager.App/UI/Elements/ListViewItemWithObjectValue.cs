using System.Collections.Generic;
using System.Windows.Forms;

namespace SimCity4.ModManager.App.UI.Elements;

public class ListViewItemWithObjectValue<T>(string text, T value) : ListViewItem(text)
{
    public T Value { get; } = value;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((ListViewItemWithObjectValue<T>)obj);
    }

    public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);

    protected bool Equals(ListViewItemWithObjectValue<T> other) =>
        EqualityComparer<T>.Default.Equals(Value, other.Value);
}