namespace MyMusicLabel.Services;

public interface IPlatformResolver
{
    string Resolve(Guid platformId);

    IReadOnlyDictionary<Guid, string> AliasesById { get; }

    IReadOnlyDictionary<string, Guid> Aliases { get; }
}
