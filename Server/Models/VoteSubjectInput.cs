namespace SimpleVote.Server.Models;

public class VoteSubjectInput
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string? VoterId { get; set; } = null;
    public DateTime? InputTime { get; set; }

    // navigational
    public VoteSubject? VoteSubject { get; set; }

    private VoteSubjectInput() { }

    private VoteSubjectInput(int subjectId, string? voterId)
    {
        SubjectId = subjectId;
        VoterId = voterId;
        InputTime = DateTime.UtcNow;
    }

    public static VoteSubjectInput Create(int subjectId, string? voterId)
        => new(subjectId, voterId);
}