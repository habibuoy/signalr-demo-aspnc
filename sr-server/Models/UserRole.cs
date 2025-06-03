namespace SignalRDemo.Server.Models;

public class UserRole
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public DateTime AssignedTime { get; set; }

    // navigational
    public User? User { get; set; }
    public Role? Role { get; set; }

    public UserRole()
    {
        Id = Guid.CreateVersion7().ToString();
        AssignedTime = DateTime.UtcNow;
    }

    public static UserRole Create(string userId, string roleId)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };
    }
}