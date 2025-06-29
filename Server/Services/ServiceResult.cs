using SimpleVote.Server.Services.Errors;

namespace SimpleVote.Server.Services;

public class ServiceResult<T> : Result<T, ServiceError>
{
    public GenericServiceErrorCode ErrorCode { get; private set; } = GenericServiceErrorCode.None;

    protected ServiceResult(bool succeded, GenericServiceErrorCode errorCode, T? value, ServiceError error)
        : base(succeded, value, error)
    {
        ErrorCode = errorCode;
    }

    public static new ServiceResult<T> Succeed(T value)
        => new(true, GenericServiceErrorCode.None, value, default!);

    public static ServiceResult<T> Fail(GenericServiceErrorCode errorCode, ServiceError error)
        => new(false, errorCode, default, error);

    public static ServiceResult<T> SimpleFail(GenericServiceErrorCode errorCode, string message)
        => new(false, errorCode, default, new(message));

    public static ServiceResult<T> SystemError(ServiceError error)
        => Fail(GenericServiceErrorCode.SystemError, error);

    public static ServiceResult<T> SimpleSystemError(string message)
        => SimpleFail(GenericServiceErrorCode.SystemError, message);

    public static ServiceResult<T> ValidationError(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        => Fail(GenericServiceErrorCode.InvalidObject,
            new ServiceValidationError("Field is in invalid state. Please check the validation errors", errors));
}