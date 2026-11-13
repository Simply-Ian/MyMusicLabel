namespace MyMusicLabel.Services;

public interface IArtistResolver
{
    bool IsOccupied(string alias);
    void Occupy(string alias, Guid artistId);
    
    IReadOnlyDictionary<string, Guid> Aliases { get; }

    IReadOnlyDictionary<Guid, string> AliasesById { get; }
}