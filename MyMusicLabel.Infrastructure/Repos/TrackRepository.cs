using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;

namespace MyMusicLabel.Infrastructure.Repos;

public class TrackRepository : ITrackRepository
{
    private readonly Dictionary<Guid, Track> _tracks = [];
    
    public Track GetTrack(Guid id) => _tracks[id];

    public void StoreTrack(Track track) => _tracks[track.Id] = track;

    public Track[] GetAllTracks() => _tracks.Values.ToArray();

    public Track[] GetTracksBySelector(Func<Track, bool> selector) => _tracks.Values.Where(selector).ToArray();

    public Track[] GetTrackList(IEnumerable<Guid> trackIds) => trackIds.Select(GetTrack).ToArray();
}