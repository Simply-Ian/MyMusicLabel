using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;

namespace MyMusicLabel.Domain.Entities;

public class StreamingPlatformDomainService(
    IStreamingPlatformAppService service, 
    ITrackRepository trackRepo,
    IArtistRepository artistRepo
) 
{
    public void Publish(Release release, Guid artistId, Guid platformId, DateOnly date)
    {
        var artist = artistRepo.GetArtist(artistId);
        if (!artist.HasRelease(release.Id))
            throw new ArgumentException($"The artist is not a performer of release {release.Title}");
        if (!artist.IsSignedToDate(date)) 
            throw new InvalidOperationException("Cannot publish the release due to that performer's contract " +
                                                "is not active to the date");

        /* Не внутри релиза, т. к. Track - отдельный агрегат. Release хранит Id-шники треков, и чтобы сделать эту же
         проверку внутри релиза, нужно передавать внутрь него (сущности) ссылку на репозиторий, что не очень хорошо */
        if (!_releaseIsReadyForPublishing(release.TrackIds)) 
            throw new ArgumentException("This release is not ready for publishing");
        
        artist.MarkNewPublishedRelease(release.Id, platformId, date);
        
        service.Publish(release, artistId, date);
        artistRepo.StoreArtist(artist);
    }
    private bool _releaseIsReadyForPublishing(IReadOnlyList<Guid> trackIds) 
        => trackIds.Any() && trackRepo.GetTrackList(trackIds).All(track => track.IsReadyForPublishing);

    public IEnumerable<int> GetTrackListeningsInPeriod(Guid trackId, DateOnly startDate, DateOnly endDate)
        => service.GetTrackListeningsInPeriod(trackId, startDate, endDate);

    public IEnumerable<decimal> GetTrackPaymentsInPeriod(Guid trackId, DateOnly startDate, DateOnly endDate) 
        => service.GetTrackPaymentsInPeriod(trackId, startDate, endDate);

    public void Revoke(Release release, Guid platformId, Guid artistId)
    {
        release.MarkRevoked(platformId); 
        service.Revoke(release.Id, artistId);
    }
}