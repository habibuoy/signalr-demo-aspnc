namespace SignalRDemo.Server.Configurations;

public class QuerySortOrderOptions
{
    public string Value { get; set; } = AppConstants.SortOrderDescendingOptionsKey;

    public static QuerySortOrderOptions Ascending
        => new() { Value = AppConstants.SortOrderAscendingOptionsKey };

    public static QuerySortOrderOptions Descending => new();
}
