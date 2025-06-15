using System.Text.Json;
using SignalRDemo.Server.Validations;
using static SignalRDemo.Server.Validations.VoteValidators;

namespace SignalRDemo.Server.Endpoints.Requests;

public record CreateVoteRequest(string Title, string[] Subjects, int? Duration, int? MaximumCount)
    : BaseRequest
{
    public override FieldValidationResult Validate(object? reference = null)
    {
        var result = FieldValidationResult.Create();

        try
        {
            if (ValidateTitle(Title) is { Succeeded: false } titleValidation)
                result.AddError(nameof(Title), titleValidation.Error);
            if (ValidateSubjects(Subjects) is { Succeeded: false } subjectsValidation)
                result.AddError(nameof(Subjects), subjectsValidation.Error);
            if (ValidateMaximumCount(MaximumCount, Subjects) is { Succeeded: false } maxCountValidation)
                result.AddError(nameof(MaximumCount), maxCountValidation.Error);
            if (ValidateDuration(Duration) is { Succeeded: false } durationValidation)
                result.AddError(nameof(Duration), durationValidation.Error);
        }
        catch (ValidatorException)
        {
            throw;
        }

        return result;
    }

    public static async ValueTask<CreateVoteRequest?> BindAsync(HttpContext httpContext)
    {
        try
        {
            var dto = await httpContext.Request.ReadFromJsonAsync<CreateVoteRequest>();
            return dto;
        }
        catch (JsonException ex)
        {
            httpContext.RequestServices.GetRequiredService<ILogger<Program>>()
                .LogInformation("Failed to parse CreateVoteDto: {msg}", ex.Message);
            return null;
        }
    }
}