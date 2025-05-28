namespace SignalRDemo.Server.Models;

public class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedTime { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}