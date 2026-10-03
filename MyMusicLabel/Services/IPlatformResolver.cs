namespace MyMusicLabel.Services;

public interface IPlatformResolver
{
    bool TryResolve(Guid platformId, out string alias);

    string Resolve(Guid platformId);

    IReadOnlyDictionary<Guid, string> AliasesById { get; }

    IReadOnlyDictionary<string, Guid> Aliases { get; }
}
