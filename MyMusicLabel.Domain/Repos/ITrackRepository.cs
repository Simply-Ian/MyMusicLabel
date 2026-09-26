using MyMusicLabel.Domain.Entities;

namespace MyMusicLabel.Domain.Repos;

public interface ITrackRepository
{
    // Throws if not found
    Track GetTrack(Guid id);
    void StoreTrack(Track track);
    Track[] GetAllTracks();
    Track[] GetTracksBySelector(Func<Track, bool> selector);
    
    // Если попадается Id несуществующего трека, должен выбросить исключение
    Track[] GetTrackList(IEnumerable<Guid> trackIds);
}