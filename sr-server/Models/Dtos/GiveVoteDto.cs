using static SignalRDemo.Server.Utils.Validators.VoteValidators;

namespace SignalRDemo.Server.Models.Dtos;

public class GiveVoteDto : BaseDto
{

    public required string VoteId { get; set; }
    public required int SubjectId { get; set; }

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