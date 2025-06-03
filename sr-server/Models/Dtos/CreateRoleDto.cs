namespace SignalRDemo.Server.Models.Dtos;

public class CreateRoleDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public CreateRoleDto(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}