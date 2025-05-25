namespace SignalRDemo.Shared;

public class VoteSubjectProperties
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public int VoteCount { get; set; }
}