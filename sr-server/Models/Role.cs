using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Models;

public class Role
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }

    public Role(string name)
    {
        Id = Guid.CreateVersion7().ToString();
        Name = name.ToLower();
        NormalizedName = Name.ToUpper();
        CreatedTime = DateTime.UtcNow;
    }

    public static Role Create(string name, string? description = null)
    {
        return new Role(name)
        {
            Description = description ?? string.Empty
        };
    }
}