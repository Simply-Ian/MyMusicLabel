using MoreLinq;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;

namespace MyMusicLabel.Domain.Services;

// Роялти не рассчитываются одновременно с прибылью, т. к. могут быть ценны сами по себе.
public class WalletDomainService
(
    IReadOnlyDictionary<Guid, StreamingPlatformDomainService> platforms,
    IArtistRepository artistRepo,
    ITrackRepository trackRepo
)
{
    public decimal ComputeTotalProfitByPeriod(DateOnly startDate, DateOnly endDate)
        => ComputeTotalIncomeByPeriod(startDate, endDate) - ComputeTotalRoyaltyByPeriod(startDate, endDate);

    public decimal ComputeTotalIncomeByPeriod(DateOnly startDate, DateOnly endDate)
        => artistRepo.GetAllArtists()
            .Sum(
                artist => artist.Releases.Sum(
                    release => platforms.Values.Sum(
                        platform => platform.GetTrackPaymentsInPeriod(release.Id, startDate, endDate).Sum()
                    )
                )
            );
    
    public decimal ComputeTotalRoyaltyByPeriod(DateOnly startDate, DateOnly endDate)
        => artistRepo.GetAllArtists()
            .Sum(
                artist => artist.Releases.Sum(release => ComputeRoyaltyForRelease(release, artist.Id, startDate, endDate))
            );

    public decimal ComputeIncomeForRelease(Release release, DateOnly startDate, DateOnly endDate)
    {
        if (!(startDate <= release.PublicationDate && release.PublicationDate <= endDate)) return 0;
        return release.TrackIds.Select(id => 
            platforms.Values.Sum(platform => platform.GetTrackPaymentsInPeriod(id, startDate, endDate).Sum()
            )
        ).Sum();
    }
    
    public decimal ComputeRoyaltyForRelease(Release release, Guid artistId, DateOnly startDate, DateOnly endDate)
    {
        if (!(startDate <= release.PublicationDate && release.PublicationDate <= endDate)) return 0;
        
        var incomesPerTrack = new Dictionary<Guid, decimal>(release.TrackIds.Select(
            id => new KeyValuePair<Guid, decimal>(id, 
                platforms.Values.Sum(
                    platform => platform.GetTrackPaymentsInPeriod(id, startDate, endDate).Sum()
                )
            )
        ));
        var totalIncomeForRelease = incomesPerTrack.Values.Sum();
        var performer = artistRepo.GetArtist(artistId);
        var performerRoyalties = performer.Releases.Contains(release) ? 
            performer.CalculatePayments(totalIncomeForRelease, startDate, endDate)
            : throw new ArgumentException("The artist does not perform this release");
        
        var tracks = trackRepo.GetTrackList(release.TrackIds);
        
        var lyricistRoyalties =
            tracks.Sum(track => track.CalculateLyricistRoyalty(incomesPerTrack[track.Id], startDate, endDate));
        var composerRoyalties =
            tracks.Sum(track => track.CalculateComposerRoyalty(incomesPerTrack[track.Id], startDate, endDate));

        return performerRoyalties + composerRoyalties + lyricistRoyalties;
    }

    public decimal ComputeRoyaltyToArtist(Guid artistId, DateOnly startDate, DateOnly endDate)
    {
        var artist = artistRepo.GetArtist(artistId);
        var performerIncome = artist.Releases.Sum(release => ComputeIncomeForRelease(release, startDate, endDate));
        var performerRoyalty = artist.CalculatePayments(performerIncome, startDate, endDate);

        decimal lyricistRoyalty = 0, composerRoyalty = 0;
        trackRepo.GetTracksBySelector(track => track.LyricistId == artistId)
            .ForEach(track =>
            {
                var totalIncomeForTrack = platforms.Values.Sum(platform =>
                    platform.GetTrackPaymentsInPeriod(track.Id, startDate, endDate).Sum()
                );
                lyricistRoyalty += track.CalculateLyricistRoyalty(totalIncomeForTrack, startDate, endDate);
                composerRoyalty += track.CalculateComposerRoyalty(totalIncomeForTrack, startDate, endDate);
            });

        return composerRoyalty + lyricistRoyalty + performerRoyalty;
    }
    
    public decimal ComputeTotalIncomeOfStyle(Style style, DateOnly startDate, DateOnly endDate) 
        => artistRepo.GetAllArtists()
            .Sum(
                artist => artist.Releases
                    .Where(release => release.Style == style)
                    .Sum(release => ComputeIncomeForRelease(release, startDate, endDate))
            );
}