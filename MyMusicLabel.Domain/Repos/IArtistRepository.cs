using MyMusicLabel.Domain.Entities;

namespace MyMusicLabel.Domain.Repos;

public interface IArtistRepository
{
    // Throws if not found
    Artist GetArtist(Guid id);
    void StoreArtist(Artist artist);

    Artist[] GetAllArtists();
}