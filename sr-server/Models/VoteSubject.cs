namespace SignalRDemo.Server.Models;

public class VoteSubject
{
    public required int Id { get; set; }
    public required string VoteId { get; set; }
    public required string Name { get; set; }

    // navigational
    public Vote? Vote { get; set; }
}