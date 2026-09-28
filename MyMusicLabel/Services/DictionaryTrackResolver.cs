namespace MyMusicLabel.Services;

public class DictionaryTrackResolver : ITrackResolver
{
    private readonly Dictionary<string, Guid> _trackAliases = new (StringComparer.OrdinalIgnoreCase);
    public bool IsOccupied(string alias) => _trackAliases.ContainsKey(alias);
    public void Occupy(string alias, Guid trackId) => _trackAliases[alias] = trackId;
    public Guid Resolve(string alias) => _trackAliases[alias];
    public IReadOnlyDictionary<string, Guid> Aliases => _trackAliases;
}
