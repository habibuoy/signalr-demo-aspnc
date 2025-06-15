namespace SignalRDemo.Server.Validations;

public class ValidatorException : Exception
{
    public ValidatorException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
        
    }
}

public class ModelFieldValidatorException : ValidatorException
{
    public string FieldName { get; init; }
    public object? FieldValue { get; init; }
    public object? ReferenceValue { get; init; }

    public ModelFieldValidatorException(string message,
        string fieldName,
        object? value,
        object? referenceValue = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        FieldName = fieldName;
        FieldValue = value;
        ReferenceValue = referenceValue;
    }
}