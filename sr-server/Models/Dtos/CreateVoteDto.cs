using System.Text.Json;
using SignalRDemo.Server.Utils.Validators;
using static SignalRDemo.Server.Utils.Validators.VoteValidators;

namespace SignalRDemo.Server.Models.Dtos;

public class CreateVoteDto : BaseDto
{
    public required string Title { get; set; } = string.Empty;
    public required string[] Subjects { get; set; } = [];
    public int? Duration { get; set; }
    public int? MaximumCount { get; set; }

    public static async ValueTask<CreateVoteDto?> BindAsync(HttpContext httpContext)
    {
        try
        {
            var dto = await httpContext.Request.ReadFromJsonAsync<CreateVoteDto>();
            return dto;
        }
        catch (JsonException ex)
        {
            httpContext.RequestServices.GetRequiredService<ILogger<Program>>()
                .LogInformation("Failed to parse CreateVoteDto: {msg}", ex.Message);
            return null;
        }
    }

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
}