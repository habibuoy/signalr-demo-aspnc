using SignalRDemo.Server.Utils.Validators;

namespace SignalRDemo.Server.Models.Dtos;

public abstract class BaseDto
{
    /// <summary>
    /// Validate the DTO
    /// </summary>
    /// <param name="reference">Object reference if any, for comparing or anything</param>
    /// <returns></returns>
    /// <exception cref="ModelFieldValidatorException"/>
    public abstract FieldValidationResult Validate(object? reference = null);
}