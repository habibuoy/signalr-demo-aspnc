namespace SignalRDemo.Server.Models.Dtos;

public class VoteSubjectDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required VoteCountDto VoteCount { get; set; } 
}