using SignalRDemo.Server.Endpoints;
using SignalRDemo.Server.Services;
using SignalRDemo.Server.Utils.Extensions;

namespace SignalRDemo.Server.Utils;

public static class HttpHelper
{
    public static IResult GetHttpResultFromServiceResult<T>(ServiceResult<T> result)
    {
        return result.ErrorCode switch
        {
            GenericServiceErrorCode.InvalidObject => Results.BadRequest(
                ResponseObject.ValidationError(
                    result.Error.TryToConvertToValidationError(out var validationError)
                        ? validationError : result.Error.Message)),
            GenericServiceErrorCode.NotFound => Results.NotFound(
                ResponseObject.Create(result.Error.Message)),
            GenericServiceErrorCode.Conflicted => Results.Conflict(
                ResponseObject.Create(result.Error.Message)),
            GenericServiceErrorCode.SystemError or _ => Results.InternalServerError(
                ResponseObject.ServerError()),
        };
    }
}