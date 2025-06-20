namespace SignalRDemo.Server.Utils.Extensions;

public static class DictionaryExtensions
{
    public static IReadOnlyDictionary<string, IReadOnlyList<string>>
        ToReadOnly(this Dictionary<string, List<string>> dictionary)
            => dictionary.ToDictionary(kv => kv.Key, kv => kv.Value as IReadOnlyList<string>);
}