using MoreLinq;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Services;

namespace MyMusicLabel.Infrastructure.Services;

/// <param name="popularity">
/// Коэффициент экспоненциального уменьшения количества прослушиваний со временем. Лучше задавать в диапазоне (0; 1]
/// </param>
public class FakeStreamingPlatformAppService(
    decimal listeningCost, 
    double popularity
) : IStreamingPlatformAppService
{
    private record Publication(Guid ReleaseId, DateOnly PublicationDate);
    
    // Ключ - Id релиза, значение - дата релиза
    private readonly Dictionary<Guid, PublishedRelease> _publishedReleases = [];

    // Кандидат на создание нового класса коллекции - MultiKeyDictionary: нескольким ключам (Id треков) соответствует
    // одно и то же значение (Id релиза)
    private readonly Dictionary<Guid, Guid> _trackToReleaseId = [];
    
    private Random _rndGen = new();
    
    public void Publish(Release release, Guid artistId, DateOnly date)
    {
        _publishedReleases.Add(release.Id, new(release, date, (uint)_rndGen.Next(50, 1000)));
        release.TrackIds.ForEach(trackId => _trackToReleaseId.Add(trackId, release.Id));
    }
    
    public IEnumerable<int> GetTrackListeningsInPeriod(Guid trackId, DateOnly startDate, DateOnly endDate)
    {
        if (!_trackToReleaseId.TryGetValue(trackId, out var releaseId)) return [];
        List<int> listeningsPerDay = [];
        for (DateOnly curDate = startDate; curDate <= endDate; curDate = curDate.AddDays(1))
        {
            int upperBound = _maxListeningsForReleaseOnDate(releaseId, curDate);
            /* Seeding the random generator to ensure reproducibility instead of generating and storing exact listenings
            count per day */
            _rndGen = new Random(curDate.DayNumber); 
            listeningsPerDay.Add(_rndGen.Next((int)(-0.1 * upperBound), (int)(0.1 * upperBound)) + upperBound);
        }
        return listeningsPerDay;
    }

    public IEnumerable<decimal> GetTrackPaymentsInPeriod(Guid releaseId, DateOnly startDate, DateOnly endDate)
        => GetTrackListeningsInPeriod(releaseId, startDate, endDate)
            .Select(listenings => listenings * ListeningCost);

    public decimal ListeningCost => listeningCost;

    private int _maxListeningsForReleaseOnDate(Guid releaseId, DateOnly date) 
    => (int)(_publishedReleases[releaseId].MaxPossibleListeningsPerDay
             * Math.Exp(-popularity * (date.DayNumber - _publishedReleases[releaseId].ReleaseDate.DayNumber)));

    public void Revoke(Guid releaseId, Guid artistId)
    {
        _publishedReleases.Remove(releaseId);
    }
}

internal record PublishedRelease(Release Release, DateOnly ReleaseDate, uint MaxPossibleListeningsPerDay);