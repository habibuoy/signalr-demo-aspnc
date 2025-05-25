namespace SignalRDemo.Shared;

public class VoteCreatedProperties
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required DateTime CreatedTime { get; set; }
    public List<VoteSubjectProperties> Subjects { get; set; } = new();
}