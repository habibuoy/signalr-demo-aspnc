using SignalRDemo.Server.Validations;
using static SignalRDemo.Server.Validations.VoteValidators;

namespace SignalRDemo.Server.Endpoints.Requests;

public record GiveVoteRequest(string VoteId, int SubjectId) : BaseRequest
{
    public override FieldValidationResult Validate(object? reference = null)
    {
        var result = FieldValidationResult.Create();
        if (ValidateVoteId(VoteId) is { Succeeded: false } voteIdValidation)
            result.AddError(nameof(VoteId), voteIdValidation.Error);

        return result;
    }

    // public static bool TryParse(HttpContext httpContext)
    // {
    //     var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    //     try
    //     {
    //         logger.LogInformation("Parsing VoteSubjectDto:");
    //         // var dto = await httpContext.Request.ReadFromJsonAsync<VoteSubjectDto>();
    //         return true;
    //     }
    //     catch (JsonException ex)
    //     {
    //         logger.LogInformation(ex.Message, "Failed to parse VoteSubjectDto:");
    //         return false;
    //     }
    // }
}