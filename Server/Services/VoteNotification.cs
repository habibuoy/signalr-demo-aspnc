using System.Threading.Channels;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Services;

public class VoteNotification : IVoteNotificationReader, IVoteNotificationWriter
{
    private readonly Channel<Vote> createdVoteChannel;
    private readonly Channel<Vote> updatedVoteChannel;

    public VoteNotification()
    {
        createdVoteChannel = Channel.CreateUnbounded<Vote>();
        updatedVoteChannel = Channel.CreateUnbounded<Vote>();
    }
    
    public async Task<Vote> ReadCreatedNotificationAsync()
    {
        while (await createdVoteChannel.Reader.WaitToReadAsync())
        {
            var result = await createdVoteChannel.Reader.ReadAsync();
            return result;
        }

        return null!;
    }

    public async Task<Vote> ReadUpdatedNotificationAsync()
    {
        while (await updatedVoteChannel.Reader.WaitToReadAsync())
        {
            var result = await updatedVoteChannel.Reader.ReadAsync();
            return result;
        }

        return null!;
    }

    public Task WriteCreateAsync(Vote vote)
    {
        return createdVoteChannel.Writer.WriteAsync(vote).AsTask();
    }

    public Task WriteUpdateAsync(Vote vote)
    {
        return updatedVoteChannel.Writer.WriteAsync(vote).AsTask();
    }

    public Task CloseAsync()
    {
        createdVoteChannel.Writer.Complete();
        updatedVoteChannel.Writer.Complete();
        return Task.CompletedTask;
    }
}