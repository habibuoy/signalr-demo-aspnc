namespace SignalRDemo.Server.Models.Dtos;

public class GiveVoteDto
{
    public required string VoteId { get; set; }
    public required int SubjectId { get; set; }

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