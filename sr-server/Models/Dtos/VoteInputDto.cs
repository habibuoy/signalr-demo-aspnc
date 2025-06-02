namespace SignalRDemo.Server.Models.Dtos;

public class VoteInputDto
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string VoteId { get; set; } = string.Empty;
    public string VoteTitle { get; set; } = string.Empty;
    public DateTime? InputTime { get; set; }
}