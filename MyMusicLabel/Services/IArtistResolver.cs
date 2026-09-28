namespace MyMusicLabel.Services;

public interface IArtistResolver
{
    bool IsOccupied(string alias);
    void Occupy(string alias, Guid artistId);
}