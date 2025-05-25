namespace SignalRDemo.Server.Models.Dtos;

public class VotesQueryDto
{
    public int? Count { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}