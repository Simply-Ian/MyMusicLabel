namespace MyMusicLabel.Services;

public class DictionaryArtistResolver : IArtistResolver
{
    private readonly Dictionary<string, Guid> _artistAliases = new (StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Guid, string> _aliasesById = [];
    public bool IsOccupied(string alias) => _artistAliases.ContainsKey(alias);
    public void Occupy(string alias, Guid artistId)
    {
        _artistAliases[alias] = artistId;
        _aliasesById[artistId] = alias;
    }
    public IReadOnlyDictionary<string, Guid> Aliases => _artistAliases;
    public IReadOnlyDictionary<Guid, string> AliasesById => _aliasesById;
}
