using MyMusicLabel.Domain.Entities;

namespace MyMusicLabel.Services;

public interface IArtistLookup
{
    bool TryGetArtist(string alias, out Artist artist);
}
