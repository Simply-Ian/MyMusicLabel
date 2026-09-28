using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;

namespace MyMusicLabel.Domain.Services;

// Роялти не рассчитываются одновременно с прибылью, т. к. могут быть ценны сами по себе.
public class WalletDomainService
(
    IServiceProvider platformsProvider,
    IArtistRepository artistRepo,
    ITrackRepository trackRepo
)
{
    private IEnumerable<IStreamingPlatformAppService> _allPlatforms =>
        platformsProvider.GetKeyedServices<IStreamingPlatformAppService>(KeyedService.AnyKey);
    private IStreamingPlatformAppService? _getPlatform(Guid platformId) =>
        platformsProvider.GetKeyedService<IStreamingPlatformAppService>(platformId);
    
    public decimal ComputeTotalProfitByPeriod(DateOnly startDate, DateOnly endDate)
        => ComputeTotalIncomeByPeriod(startDate, endDate) 
           - ComputeTotalRoyaltyByPeriod(startDate, endDate);
    public decimal ComputeTotalIncomeByPeriod(DateOnly startDate, DateOnly endDate, Guid? platformId = null)
        => trackRepo.GetAllTracks()
            .Sum(
                track => platformId == null? 
                    _allPlatforms.Sum(
                        platform => platform.GetTrackPaymentsInPeriod(track.Id, startDate, endDate).Sum()
                    )
                    : _getPlatform(platformId.Value)
                        ?.GetTrackPaymentsInPeriod(track.Id, startDate, endDate).Sum() ?? 0
                );
    
    public decimal ComputeTotalRoyaltyByPeriod(DateOnly startDate, DateOnly endDate)
        => artistRepo.GetAllArtists()
            .Sum(
                artist => artist.Releases.Sum(
                    release => ComputeTotalRoyaltyForRelease(release, artist.Id, startDate, endDate)
                )
            );
    
    /// <returns>Сумму выплат за период всем артистам, кто принимал участие в работе над релизом: композиторам и авторам
    /// текстов для всех треков, а также исполнителю</returns>
    public decimal ComputeTotalRoyaltyForRelease(
        Release release, 
        Guid performerId, 
        DateOnly startDate, 
        DateOnly endDate
    )
    {
        /* TODO: трек может быть включен в несколько релизов, и интерфейс стриминговой платформы должен, по-хорошему,
        учитывать это. Исправить так же в ComputeIncomeForRelease */
        var incomesPerTrack = new Dictionary<Guid, decimal>(release.TrackIds.Select(
            id => new KeyValuePair<Guid, decimal>(id, 
                _allPlatforms.Sum(
                    platform => platform.GetTrackPaymentsInPeriod(id, startDate, endDate).Sum()
                )
            )
        ));
        var totalIncomeForRelease = incomesPerTrack.Values.Sum();
        var performer = artistRepo.GetArtist(performerId);
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
    
    public decimal ComputeIncomeForRelease(Release release, DateOnly startDate, DateOnly endDate)
    {
        return release.TrackIds.Select(id => 
            _allPlatforms.Sum(platform => platform.GetTrackPaymentsInPeriod(id, startDate, endDate).Sum()
            )
        ).Sum();
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
                var totalIncomeForTrack = _allPlatforms.Sum(platform =>
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