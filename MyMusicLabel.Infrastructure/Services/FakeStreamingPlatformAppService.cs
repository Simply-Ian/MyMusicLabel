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
    // Ключ - Id релиза, значение - дата релиза
    private readonly Dictionary<Guid, PublishedRelease> _publishedReleases = [];

    // Кандидат на создание нового класса коллекции - MultiKeyDictionary: нескольким ключам (Id треков) соответствует
    // одно и то же значение (Id релиза)
    private readonly Dictionary<Guid, Guid> _trackToReleaseId = [];
    
    public void Publish(Release release, Guid artistId, DateOnly date)
    {
        Random rndGen = new(date.DayNumber);
        _publishedReleases.Add(release.Id, new(release, date, (uint)rndGen.Next(50, 1000)));
        release.TrackIds.ForEach(trackId => _trackToReleaseId.Add(trackId, release.Id));
    }
    
    public IEnumerable<int> GetTrackListeningsInPeriod(Guid trackId, DateOnly startDate, DateOnly endDate)
    {
        if (!_trackToReleaseId.TryGetValue(trackId, out var releaseId)) return [];
        if (!_publishedReleases.TryGetValue(releaseId, out var publishedRelease)) return [];
        if (startDate < publishedRelease.ReleaseDate) startDate = publishedRelease.ReleaseDate;
        List<int> listeningsPerDay = [];
        for (DateOnly curDate = startDate; curDate <= endDate; curDate = curDate.AddDays(1))
        {
            int upperBound = _maxListeningsForReleaseOnDate(publishedRelease, curDate);
            /* Seeding the random generator to ensure reproducibility instead of generating and storing exact listenings
            count per day */
            var rndGen = new Random(curDate.DayNumber); 
            listeningsPerDay.Add(rndGen.Next((int)(-0.1 * upperBound), (int)(0.1 * upperBound)) + upperBound);
        }
        return listeningsPerDay;
    }

    public IEnumerable<decimal> GetTrackPaymentsInPeriod(Guid trackId, DateOnly startDate, DateOnly endDate)
        => GetTrackListeningsInPeriod(trackId, startDate, endDate)
            .Select(listenings => listenings * ListeningCost);

    public decimal ListeningCost => listeningCost;

    private int _maxListeningsForReleaseOnDate(PublishedRelease publishedRelease, DateOnly date) 
    => (int)(publishedRelease.MaxPossibleListeningsPerDay
             * Math.Exp(-popularity * (date.DayNumber - publishedRelease.ReleaseDate.DayNumber)));

    public void Revoke(Guid releaseId, Guid artistId)
    {
        if (!_publishedReleases.Remove(releaseId, out var publishedRelease)) return;
        foreach (var trackId in publishedRelease.Release.TrackIds) _trackToReleaseId.Remove(trackId);
    }
}

internal record PublishedRelease(Release Release, DateOnly ReleaseDate, uint MaxPossibleListeningsPerDay);