using System.Collections.Concurrent;
using SignalRDemo.Server.Utils;

namespace SignalRDemo.Server.Services;

public class HubConnectionManager : IHubConnectionManager
{
    private static readonly ConcurrentDictionary<string, ConcurrentHashSet<string>> connectionIds = new();
    private readonly ILogger<HubConnectionManager> logger;

    public HubConnectionManager(ILogger<HubConnectionManager> logger)
    {
        this.logger = logger;
    }

    public Task<bool> AddConnectionIdAsync(string userId, string connectionId)
    {
        var hashSet = connectionIds.GetOrAdd(userId, value: new());
        var success = hashSet.TryAdd(connectionId, out var count);
        if (success)
        {
            logger.LogInformation("Added conn id {connectionId} to {userId}, count: {c}",
                connectionId, userId, count);
        }

        return Task.FromResult(success);
    }

    public Task<bool> RemoveConnectionIdAsync(string userId, string connectionId)
    {
        var hashSet = connectionIds.GetOrAdd(userId, value: new());

        var success = hashSet.TryRemove(connectionId, out var count);
        if (success)
        {
            logger.LogInformation("Removed conn id {connectionId} from {userId}, count: {c}",
                connectionId, userId, count);
        }

        return Task.FromResult(success);
    }

    public Task<IEnumerable<string>> GetConnectionIdsAsync(string userId)
    {
        var hashSet = connectionIds.GetOrAdd(userId, value: new());
        return Task.FromResult<IEnumerable<string>>(hashSet.ToArray());
    }
}