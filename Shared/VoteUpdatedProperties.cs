namespace SignalRDemo.Shared;

public class VoteUpdatedProperties
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required int TotalCount { get; set; }
    public List<VoteSubjectProperties> Subjects { get; set; } = new();
}