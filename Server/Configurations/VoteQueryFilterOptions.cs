using System.Linq.Expressions;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Configurations;

public class VoteQueryFilterOptions
{
    public Dictionary<string, Expression<Func<Vote, object>>> SorterExpressions { get; set; } = null!;
    public Expression<Func<Vote, object>> DefaultSorterExpression { get; set; } = null!;
    public QuerySortOrderOptions DefaultSortOrderOption { get; set; } = null!;
    public Dictionary<string, object> SortBy { get; set; } = null!;
    public Dictionary<string, object> SortOrder { get; set; } = null!;
    public Dictionary<string, object> Search { get; set; } = null!;
}