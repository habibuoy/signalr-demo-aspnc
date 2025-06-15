namespace SignalRDemo.Server.Validations;

public class FieldValidationResult : Result<string, Dictionary<string, List<string>>>
{
    private FieldValidationResult(bool succeded)
        : base(succeded, null, new()) { }

    /// <summary>
    /// Add error and set the success value to false
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="errors"></param>
    public void AddError(string fieldName, IEnumerable<string> errors)
    {
        if (Succeeded) Succeeded = false;
        
        if (!Error.TryGetValue(fieldName, out var fieldErrors))
        {
            fieldErrors = new();
            Error.Add(fieldName, fieldErrors);
        }

        fieldErrors.AddRange(errors);
    }


    /// <summary>
    /// Create a new validation with successful result
    /// </summary>
    /// <returns></returns>
    public static FieldValidationResult Create()
    {
        return new(true);
    }
}