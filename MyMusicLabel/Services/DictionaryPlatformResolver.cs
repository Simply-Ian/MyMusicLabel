namespace MyMusicLabel.Services;

public class DictionaryPlatformResolver : IPlatformResolver
{
    private readonly Dictionary<Guid, string> _aliasesById;

    public DictionaryPlatformResolver(Dictionary<string, Guid> aliases)
    {
        _aliasesById = aliases.ToDictionary(pair => pair.Value, pair => pair.Key);
        Aliases = new Dictionary<string, Guid>(aliases, StringComparer.OrdinalIgnoreCase);
    }

    public string Resolve(Guid platformId) => _aliasesById[platformId];

    public IReadOnlyDictionary<Guid, string> AliasesById => _aliasesById;

    public IReadOnlyDictionary<string, Guid> Aliases { get; }
}
