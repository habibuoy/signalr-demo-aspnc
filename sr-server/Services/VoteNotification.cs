using System.Threading.Channels;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Services;

public class VoteNotification : IVoteNotificationReader, IVoteNotificationWriter
{
    private readonly Channel<Vote> voteChannel;

    public VoteNotification()
    {
        voteChannel = Channel.CreateUnbounded<Vote>();
    }

    public async Task<Vote> ReadAsync()
    {
        while (await voteChannel.Reader.WaitToReadAsync())
        {
            var result = await voteChannel.Reader.ReadAsync();
            return result;
        }

        return null!;
    }

    public Task WriteUpdateAsync(Vote vote)
    {
        return voteChannel.Writer.WriteAsync(vote).AsTask();
    }

    public Task CloseAsync()
    {
        voteChannel.Writer.Complete();
        return Task.CompletedTask;
    }
}