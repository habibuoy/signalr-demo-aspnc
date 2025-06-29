using System.Threading.Channels;
using SimpleVote.Server.Interfaces;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Services;

public class VoteQueue : IVoteQueueWriter, IVoteQueueReader
{
    private readonly Channel<VoteQueueItem> voteQueueChannel;
    private readonly ChannelWriter<VoteQueueItem> voteQueueWriter;
    private readonly ChannelReader<VoteQueueItem> voteQueueReader;

    public VoteQueue()
    {
        voteQueueChannel = Channel.CreateUnbounded<VoteQueueItem>();
        voteQueueWriter = voteQueueChannel.Writer;
        voteQueueReader = voteQueueChannel.Reader;
    }

    public async Task<VoteQueueItem> ReadAsync()
    {
        while (await voteQueueReader.WaitToReadAsync())
        {
            return await voteQueueReader.ReadAsync();
        }

        return default;
    }

    public async Task WriteAsync(VoteQueueItem vote)
    {
        await voteQueueWriter.WriteAsync(vote);
    }

    public Task CloseAsync()
    {
        voteQueueWriter.Complete();
        return Task.CompletedTask;
    }
}
