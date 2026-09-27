using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;

namespace MyMusicLabel.Infrastructure.Repos;

public class ArtistRepository : IArtistRepository
{
    private readonly Dictionary<Guid, Artist> _artists = [];
    
    public Artist GetArtist(Guid id) => _artists[id];

    public void StoreArtist(Artist artist) => _artists[artist.Id] = artist;

    public Artist[] GetAllArtists() => _artists.Values.ToArray();
}