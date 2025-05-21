namespace SignalRDemo.Server.Models;

public class VoteCount
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public int Count { get; set; }

    // for concurrency
    public Guid Version { get; set; }

    // navigational
    public VoteSubject? VoteSubject { get; set; }
}