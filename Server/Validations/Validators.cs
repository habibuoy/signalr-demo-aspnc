namespace SimpleVote.Server.Validations;

public static class Validators
{
    /// <summary>
    /// Validate a field with type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Field type</typeparam>
    /// <param name="value">Field value</param>
    /// <param name="validateAction">Validate action, when error happens, pass the string error to the list</param>
    /// <returns><see cref="Result{T, List{T}(string)}"/> object with value of type <typeparamref name="T"/> if validation succeeded
    /// or List of error message otherwise</returns>
    /// <exception cref="ModelFieldValidatorException{T}"></exception>
    public static Result<T, List<string>> ValidateModelFieldValue<T>(string fieldName,
        T value, Action<T, List<string>> validateAction)
    {
        try
        {
            var errors = new List<string>();
            validateAction.Invoke(value, errors);

            if (errors.Count > 0)
            {
                return Result<T, List<string>>.Fail(errors);
            }

            return Result<T, List<string>>.Succeed(value);
        }
        catch (Exception ex)
        {
            throw new ModelFieldValidatorException("Unexpected error happened while validating model field",
                fieldName, value, innerException: ex);
        }
    }

    /// <summary>
    /// Validate a field with type <typeparamref name="T"/> and object reference with type <typeparamref name="TReference"/>
    /// </summary>
    /// <typeparam name="TReference">Object reference type</typeparam>
    /// <param name="reference">Reference object value</param>
    /// <inheritdoc cref="ValidateModelFieldValue"/>
    public static Result<T, List<string>> ValidateModelFieldValue<T, TReference>(string fieldName,
        T value, TReference reference, Action<T, TReference, List<string>> validateAction)
    {
        try
        {
            var errors = new List<string>();
            validateAction.Invoke(value, reference, errors);

            if (errors.Count > 0)
            {
                return Result<T, List<string>>.Fail(errors);
            }

            return Result<T, List<string>>.Succeed(value);
        }
        catch (Exception ex)
        {
            throw new ModelFieldValidatorException("Unexpected error happened while validating model field",
                fieldName, value, reference, ex);
        }
    }
}