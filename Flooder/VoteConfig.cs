namespace SignalRDemo.Flooder;

public class VoteConfig
{
    public int SubjectCount { get; set; } = 2;
    public int? Duration { get; set; }
    public int? MaximumCount { get; set; }

    public static VoteConfig Default => new ()
    {
        SubjectCount = 2,
        Duration = 60,
        MaximumCount = null
    };
}