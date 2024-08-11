namespace SimCity4.ModManager;

public class ToStringDecorator<T>(T objectValue, string stringValue)
{
    public T ObjectValue { get; set; } = objectValue;

    public string StringValue { get; set; } = stringValue;

    public override string ToString() => StringValue;
}