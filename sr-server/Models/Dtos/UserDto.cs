namespace SignalRDemo.Server.Models.Dtos;

public class UserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}