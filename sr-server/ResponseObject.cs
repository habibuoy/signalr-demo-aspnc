namespace SignalRDemo.Server.Response;

public class ResponseObject
{
    public string Message { get; set; } = string.Empty;
    public object? Result { get; set; }

    /// <summary>
    /// Return a response object with given message and result (default null)
    /// </summary>
    /// <param name="message"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static ResponseObject Create(string message, object? result = null)
    {
        return new ResponseObject()
        {
            Message = message,
            Result = result
        };
    }

    /// <summary>
    /// Return a response object with empty message and a result
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static ResponseObject Success(object result)
    {
        return new ResponseObject()
        {
            Message = "",
            Result = result
        };
    }

    /// <summary>
    /// Return a response object with bad request body message and null result
    /// </summary>
    /// <returns></returns>
    public static ResponseObject BadBody()
    {
        return Create("Please provide a valid request body");
    }

    /// <summary>
    /// Return a response object with bad request query parameter message and null result
    /// </summary>
    /// <returns></returns>
    public static ResponseObject BadQuery()
    {
        return Create("Please provide a valid request query parameters");
    }

    /// <summary>
    /// Return a response object with resource not found message and null result
    /// </summary>
    /// <returns></returns>
    public static ResponseObject NotFound()
    {
        return Create("Resource not found");
    }
}