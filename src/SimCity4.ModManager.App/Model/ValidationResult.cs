namespace SimCity4.ModManager.App.Model;

public class ValidationResult(bool valid, string[] errorMessages)
{
    public ValidationResult(bool valid, string errorMessage) : this(valid, new[] { errorMessage })
    {
    }

    public bool Valid { get; private set; } = valid;

    public string[] ErrorMessages { get; private set; } = errorMessages;
}