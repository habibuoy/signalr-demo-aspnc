using System.Linq.Expressions;
using SimpleVote.Server.Interfaces;
using SimpleVote.Server.Models;
using SimpleVote.Server.Services;
using static SimpleVote.Server.Configurations.AppConstants;

namespace SimpleVote.Server.Configurations;

public static class DomainConfigurations
{
    private const string VoteTitleFilterOptionKey = "ttl";
    private const string VoteCreatedTimeFilterOptionKey = "cdt";
    private const string VoteMaximumCountFilterOptionsKey = "mxc";
    private const string VoteExpiredTimeFilterOptionKey = "exp";
    private const string VoteCreatorFilterOptionKey = "ctr";

    private static readonly Dictionary<string, Expression<Func<Vote, object>>> VoteSorterExpressions = new()
    {
        { VoteTitleFilterOptionKey, vote => vote.Title },
        { VoteCreatedTimeFilterOptionKey, vote => vote.CreatedTime },
        { VoteMaximumCountFilterOptionsKey, vote => vote.MaximumCount! },
        { VoteExpiredTimeFilterOptionKey, vote => vote.ExpiredTime! },
    };

    private static readonly Dictionary<string, object> VoteFilterSortByOptions = new()
    {
        { VoteTitleFilterOptionKey, new { NormalizedName = "Title" } },
        { VoteCreatedTimeFilterOptionKey, new { NormalizedName = "Created Time" } },
        { VoteMaximumCountFilterOptionsKey, new { NormalizedName = "Maximum Count" } },
        { VoteExpiredTimeFilterOptionKey, new { NormalizedName = "Expired Time" } },
    };

    private static readonly Dictionary<string, object> VoteFilterSearchOptions = new()
    {
        { VoteTitleFilterOptionKey, new { NormalizedName = "Title" } },
        { VoteCreatorFilterOptionKey, new { NormalizedName = "Creator" } },
    };

    private static readonly Dictionary<string, object> FilterSortOrderOptions = new()
    {
        { SortOrderAscendingOptionsKey, new { NormalizedName = "Ascending" } },
        { SortOrderDescendingOptionsKey, new { NormalizedName = "Descending" } },
    };

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IVoteService, DbVoteService>();
        services.AddScoped<IVoteQueueService, DbVoteService>();
        services.AddSingleton<IHubConnectionManager, HubConnectionManager>();
        services.AddSingleton<IHubConnectionReader, HubConnectionManager>();

        var voteQueue = new VoteQueue();
        services.AddSingleton<IVoteQueueReader>(voteQueue);
        services.AddSingleton<IVoteQueueWriter>(voteQueue);

        var voteNotification = new VoteNotification();
        services.AddSingleton<IVoteNotificationReader>(voteNotification);
        services.AddSingleton<IVoteNotificationWriter>(voteNotification);

        services.AddHostedService<VoteQueueProcessorBackgroundService>();
        services.AddHostedService<VoteBroadcasterBackgroundService>();

        return services;
    }

    public static IServiceCollection AddOptionServices(this IServiceCollection services)
    {
        services.Configure<VoteQueryFilterOptions>(configure =>
        {
            configure.SorterExpressions = VoteSorterExpressions;
            configure.DefaultSorterExpression = VoteSorterExpressions[VoteCreatedTimeFilterOptionKey];
            configure.DefaultSortOrderOption = QuerySortOrderOptions.Descending;
            configure.SortBy = VoteFilterSortByOptions;
            configure.SortOrder = FilterSortOrderOptions;
            configure.Search = VoteFilterSearchOptions;
        });

        return services;
    }
}