namespace SignalRDemo.Server;

/// <summary>
/// Result to any operation
/// </summary>
/// <typeparam name="T">Object type that is produced by successful operation</typeparam>
/// <typeparam name="TError">Error object type that is produced by failed operation</typeparam>
public class Result<T, TError>
{
    protected readonly T? value;
    protected readonly TError? error;

    public bool Succeeded { get; protected set; }
    public T Value => Succeeded ? value! : throw new InvalidOperationException("Result is not successful");
    public TError Error => !Succeeded ? error! : throw new InvalidOperationException("No error, result is successful");

    protected Result(bool succeded, T? value, TError? error)
    {
        Succeeded = succeded;
        this.value = value;
        this.error = error;
    }

    public static Result<T, TError> Succeed(T value) => new(true, value, default);
    public static Result<T, TError> Fail(TError error) => new(false, default, error);
}