namespace MyMusicLabel.Services;

public interface IArtistResolver
{
    bool IsOccupied(string alias);
    void Occupy(string alias, Guid artistId);
    
    Guid Resolve(string alias);
    
    IReadOnlyDictionary<string, Guid> Aliases { get; }
}