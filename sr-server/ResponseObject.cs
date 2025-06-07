namespace SignalRDemo.Server.Responses;

public class ResponseObject
{
    public string Message { get; set; } = string.Empty;
    public object? Result { get; set; }

    private ResponseObject() { }

    private ResponseObject(string message, object? result)
    {
        Message = message;
        Result = result;
    }

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

    /// <summary>
    /// Return a response object with resource not authorized message and null result
    /// </summary>
    /// <returns></returns>
    public static ResponseObject NotAuthorized()
    {
        return Create("You are not authorized to access this resource");
    }

    /// <summary>
    /// Return a response object with validation error message and result containing of error fields
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static ResponseObject ValidationError(object result)
    {
        return Create("Validation Error. Please correct the invalid field(s)", new { validationErrors = result });
    }

    /// <summary>
    /// Return a response object with resource concurrency message and null result
    /// </summary>
    /// <returns></returns>
    public static ResponseObject Concurrency()
    {
        return Create("Resource was updated by someone else while updating your request");
    }

    /// <summary>
    /// Return a response object with internal server error message and null result
    /// </summary>
    /// <returns></returns>
    public static ResponseObject ServerError()
    {
        return Create("There was an error on our side");
    }
}