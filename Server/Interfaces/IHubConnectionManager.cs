namespace SignalRDemo.Server;

public interface IHubConnectionManager : IHubConnectionReader
{
    Task<bool> AddConnectionIdAsync(string userId, string connectionId);
    Task<bool> RemoveConnectionIdAsync(string userId, string connectionId);
}

public interface IHubConnectionReader
{
    Task<IEnumerable<string>> GetConnectionIdsAsync(string userId);
}