using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Exceptions;
using MyMusicLabel.Domain.Repos;

namespace MyMusicLabel.Domain.Services;

public class StreamingPlatformDomainService(
    IStreamingPlatformAppService service, 
    ITrackRepository trackRepo,
    IArtistRepository artistRepo
) 
{
    public void Publish(Release release, Guid artistId, Guid platformId, DateOnly date)
    {
        var artist = artistRepo.GetArtist(artistId);

        /* Не внутри релиза, т. к. Track - отдельный агрегат. Release хранит Id-шники треков, и чтобы сделать эту же
         проверку внутри релиза, нужно передавать внутрь него (сущности) ссылку на репозиторий, что не очень хорошо */
        if (!_releaseIsReadyForPublishing(release.TrackIds)) throw new ReleaseNotReadyException(release);
        
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

    // throws ReleaseNotPublishedException
    public void Revoke(Release release, Guid platformId, Guid artistId)
    {
        release.MarkRevoked(platformId); 
        service.Revoke(release.Id, artistId);
    }
}