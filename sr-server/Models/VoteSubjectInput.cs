namespace SignalRDemo.Server.Models;

public class VoteSubjectInput
{
    private static int LastId = 0;

    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string? VoterId { get; set; } = null;
    public DateTime? InputTime { get; set; }

    // navigational
    public VoteSubject? VoteSubject { get; set; }

    public VoteSubjectInput()
    {
        Id = GetId();
    }

    public static int GetId()
    {
        return ++LastId;
    }
}