namespace MyMusicLabel.Services;

public class DictionaryArtistResolver : IArtistResolver
{
    private readonly Dictionary<string, Guid> _artistAliases = new (StringComparer.OrdinalIgnoreCase);
    public bool IsOccupied(string alias) => _artistAliases.ContainsKey(alias);
    public void Occupy(string alias, Guid artistId) => _artistAliases[alias] = artistId;
}
