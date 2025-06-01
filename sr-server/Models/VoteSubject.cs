namespace SignalRDemo.Server.Models;

public class VoteSubject
{
    public int Id { get; set; }
    public string VoteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Guid? Version { get; set; }

    public List<VoteSubjectInput> Voters { get; set; } = new();

    // navigational
    public Vote? Vote { get; set; }
}