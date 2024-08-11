namespace SimCity4.ModManager.App.UI.Elements;

public class ComboBoxItem<T>(string text, T value)
{
    public string Text { get; set; } = text;

    public T Value { get; set; } = value;

    public override string ToString() => Text;
}