namespace SignalRDemo.Shared;

public class VoteCreatedProperties
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public int SubjectCount { get; set; }
}