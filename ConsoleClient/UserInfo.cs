namespace SignalRDemo.ConsoleClient;

public class UserInfo
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public static UserInfo Default => new ()
    {
        Email = "defaultuser@gmail.com",
        Password = "Password123@",
        FirstName = "Default",
        LastName = "User",
    };
}