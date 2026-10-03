namespace MyMusicLabel.Services;

public class DictionaryTrackResolver : ITrackResolver
{
    private readonly Dictionary<string, Guid> _trackAliases = new (StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Guid, string> _aliasesById = [];
    public bool IsOccupied(string alias) => _trackAliases.ContainsKey(alias);
    public void Occupy(string alias, Guid trackId)
    {
        _trackAliases[alias] = trackId;
        _aliasesById[trackId] = alias;
    }
    public Guid Resolve(string alias) => _trackAliases[alias];
    public IReadOnlyDictionary<string, Guid> Aliases => _trackAliases;
    public IReadOnlyDictionary<Guid, string> AliasesById => _aliasesById;
}
