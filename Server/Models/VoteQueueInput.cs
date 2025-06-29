namespace SignalRDemo.Server.Models;

public class VoteQueueInput
{
    public string Id { get; private set; } = string.Empty;
    public string VoteId { get; private set; } = string.Empty;
    public string SubjectId { get; private set; } = string.Empty;
    public string? VoterId { get; private set; } = string.Empty;
    public VoteQueueInputStatus Status { get; private set; } = VoteQueueInputStatus.Processing;
    public DateTime InputTime { get; private set; }
    public DateTime? ProcessedTime { get; private set; }
    public string StatusDetail { get; private set; } = string.Empty;

    private VoteQueueInput() { }

    private VoteQueueInput(string voteId, string subjectId, string? voterId)
    {
        Id = Guid.CreateVersion7().ToString();
        VoterId = voterId;
        VoteId = voteId;
        SubjectId = subjectId;
        InputTime = DateTime.UtcNow;
        StatusDetail = "Vote input is being processed";
    }

    public static VoteQueueInput Create(string voteId, string subjectId, string? VoterId)
        => new(voteId, subjectId, VoterId);
}

public enum VoteQueueInputStatus
{
    Processing,
    Finished,
    Failed
}