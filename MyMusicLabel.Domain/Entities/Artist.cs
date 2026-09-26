using MyMusicLabel.Domain.Contracts;

namespace MyMusicLabel.Domain.Entities;

/*
 * Артист - это Entity в любом случае
 * Артист может быть автором музыки и текстов для разных треков => получать выплаты по одноразовым контрактам
 * Артист может быть исполнителем разных релизов (и, соответственно, треков) => получать выплаты по постоянному контракту
 * Соответственно, если автор музыки/текста для нас - только Id и имя, а исполнитель имеет разные релизы + постоянный
 * контракт, но при этом ИРЛ это одно и то же лицо, нам надо вводить 2 Bounded Context-а: один для записи треков и
 * один - для публикации.
 * Однако нам лень, поэтому постоянный контракт сделаем nullable.
 */
public class Artist (Guid id, string name, PersistentContract? contract = null) : BaseEntity(id)
{
    private PersistentContract? _contract = contract;
    
    public string Name { get; set; } = name;
    
    private readonly List<Release> _releases = [];
    public IReadOnlyList<Release> Releases => _releases;

    public void Drop(DateOnly date) => _contract = _contract?.Break(date);
    public bool IsSignedToDate(DateOnly date) => _contract?.IsActiveToDate(date) ?? false;
    public bool HasRelease(Guid releaseId) => _releases.Any(r => r.Id == releaseId);

    /// <summary>
    /// Не публикует релиз, просто добавляет его в список
    /// </summary>
    /// <param name="release"></param>
    public void AddRelease(Release release)
    {
        if (release.IsPublishedAnywhere)
            throw new InvalidOperationException($"Cannot assign already published release to another artist");
        if (Releases.Contains(release)) throw new ArgumentException("The artist is already performs this release");
        
        _releases.Add(release);
    }

    public void RemoveRelease(Guid releaseId)
    {
        var release = _releases.First(r => r.Id == releaseId) 
                      ?? throw new ArgumentException("The artist does not perform this release");
        if (release.IsPublishedAnywhere)
            throw new InvalidOperationException($"Cannot remove already published release");
        if (!Releases.Contains(release)) throw new ArgumentException("The artist does not perform this release");
        
        _releases.Remove(release);
    }

    public void MarkNewPublishedRelease(Guid releaseId, Guid platformId, DateOnly date)
    {
        if (_contract is null) 
            throw new InvalidOperationException($"Cannot publish a release performed by an unsigned artist");
        var release = _releases.First(r => r.Id == releaseId) 
                      ?? throw new ArgumentException("The artist does not perform this release");
        if (!release.IsPublishedAnywhere) _contract?.AddNewRelease(date);
        release.MarkPublished(platformId, date);
    }

    public void AddTrackToRelease(Guid releaseId, Track track)
    {
        if (track.PerformerId != Id) 
            throw new ArgumentException("Release by a specific artist cannot contain tracks performed by another artist");
        var release = _releases.First(r => r.Id == releaseId) 
                      ?? throw new ArgumentException("The artist does not perform this release");
        release.AddTrack(track.Id);
    }

    public void RemoveTrackFromRelease(Guid releaseId, Guid trackId)
    {
        var release = _releases.First(r => r.Id == releaseId) 
                      ?? throw new ArgumentException("The artist does not perform this release");
        release.RemoveTrack(trackId);
    }
    
    public decimal CalculatePayments(decimal totalIncomeForRelease, DateOnly startDate, DateOnly endDate)
        => _contract?.CalculatePayments(totalIncomeForRelease, startDate, endDate) 
           ?? throw new InvalidOperationException("The artist is not signed and thus can not receive any royalties");
}