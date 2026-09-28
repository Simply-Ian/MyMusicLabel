using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;

namespace MyMusicLabel.Services;

public class ArtistLookup(IArtistResolver aliases, IArtistRepository artistRepo) : IArtistLookup
{
    public bool TryGetArtist(string alias, out Artist artist)
    {
        artist = null!;
        if (!aliases.Aliases.TryGetValue(alias, out var artistId)) return false;

        artist = artistRepo.GetArtist(artistId);
        return true;
    }
}
