namespace SignalRDemo.Server.Utils.Validators;

public class ValidatorException : Exception
{
    public ValidatorException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
        
    }
}

public class ModelFieldValidatorException : ValidatorException
{
    public object? FieldValue { get; init; }
    public object? ReferenceValue { get; init; }

    public ModelFieldValidatorException(string message,
        object? value,
        object? referenceValue = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        FieldValue = value;
        ReferenceValue = referenceValue;
    }
}