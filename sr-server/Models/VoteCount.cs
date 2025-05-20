namespace SignalRDemo.Server.Models;

public class VoteCount
{
    public required int Id { get; set; }
    public required string VoteId { get; set; }
    public required int Count { get; set; }

    // navigational
    public Vote? Vote { get; set; }
}