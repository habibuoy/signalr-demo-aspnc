using SimpleVote.Server.Validations;

namespace SimpleVote.Server.Endpoints.Requests;

public abstract record BaseRequest
{
    /// <summary>
    /// Validate the DTO
    /// </summary>
    /// <param name="reference">Object reference if any, for comparing or anything</param>
    /// <returns></returns>
    /// <exception cref="ModelFieldValidatorException"/>
    public abstract FieldValidationResult Validate(object? reference = null);
}