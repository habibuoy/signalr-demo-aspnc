namespace SignalRDemo.Server.Services;

public class ServiceResult<T> : Result<T, IEnumerable<string>>
{
    public GenericServiceErrorCode ErrorCode { get; private set; } = GenericServiceErrorCode.None;

    protected ServiceResult(bool succeded, GenericServiceErrorCode errorCode, T? value, IEnumerable<string>? error)
        : base(succeded, value, error)
    {
        ErrorCode = errorCode;
    }

    public static new ServiceResult<T> Succeed(T value)
        => new(true, GenericServiceErrorCode.None, value, null);

    public static ServiceResult<T> Fail(GenericServiceErrorCode errorCode, IEnumerable<string> error)
        => new(false, errorCode, default, error);

    public static ServiceResult<T> SystemError(IEnumerable<string> error)
        => Fail(GenericServiceErrorCode.SystemError, error);
}