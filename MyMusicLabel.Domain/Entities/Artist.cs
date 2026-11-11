using MyMusicLabel.Domain.Contracts;
using MyMusicLabel.Domain.Exceptions;

namespace MyMusicLabel.Domain.Entities;

public class Artist (Guid id, string name) : BaseEntity(id)
{
    private PersistentContract? _contract;
    
    public string Name { get; set; } = name;
    
    private readonly List<Release> _releases = [];
    public IReadOnlyList<Release> Releases => _releases;

    public DateOnly? SignDate => _contract?.SignDate;
    public DateOnly? DropDate => _contract?.BreakDate;
    
    /// <summary>
    /// Подписывает артиста на лейбл с тем условием, что за опубликованные релизы артист будет получать процентные
    /// отчисления
    /// </summary>
    /// <param name="charge">Ставка процентных отчислений в диапазоне 0.0 - 1.0</param>
    /// <param name="signDate">Дата подписания контракта</param>
    /// <param name="plannedReleasesCount">Количество релизов, которые артист должен выпустить по условиям контракта</param>
    public void SignPerformerForCharge(decimal charge, DateOnly signDate, uint plannedReleasesCount)
        => _contract = new PersistentContract(new ChargedContract(charge), signDate, plannedReleasesCount);
    
    /// <summary>
    /// Подписывает артиста на лейбл с тем условием, что за первую публикацию нового релиза артист будет получать
    /// фиксированную сумму
    /// </summary>
    /// <param name="price">Сумма выплат за первую публикацию нового релиза, р</param>
    /// <param name="signDate">Дата подписания контракта</param>
    /// <param name="plannedReleasesCount">Количество релизов, которые артист должен выпустить по условиям контракта</param>
    public void SignPerformerForFixedSum(decimal price, DateOnly signDate, uint plannedReleasesCount)
        => _contract = new PersistentContract(new FixedSumContract(price), signDate, plannedReleasesCount);
    
    /// <summary>
    /// Разрывает контракт с артистом в указанную дату
    /// </summary>
    public void Drop(DateOnly date) => _contract = _contract?.Break(date);
    public bool IsSignedToDate(DateOnly date) => _contract?.IsActiveToDate(date) ?? false;
    
    public bool HasRelease(Guid releaseId) => _releases.Any(r => r.Id == releaseId);

    /// <summary>
    /// Не публикует релиз, просто добавляет его в список
    /// </summary>
    /// <param name="release"></param>
    public void AddRelease(Release release)
    {
        if (Releases.Contains(release)) throw new AlreadyPerformsException();
        // Релиз опубликован от имени другого артиста
        if (release.IsPublishedAnywhere) throw new ArtistMismatchException();
        _releases.Add(release);
    }

    public void RemoveRelease(Guid releaseId)
    {
        var release = _releases.First(r => r.Id == releaseId) ?? throw new ReleaseNotFoundException(releaseId);
        if (release.IsPublishedAnywhere)
            throw new InvalidOperationException($"Cannot remove already published release");
        _releases.Remove(release);
    }

    internal void MarkNewPublishedRelease(Guid releaseId, Guid platformId, DateOnly date)
    {
        if (_contract is null || !_contract.IsActiveToDate(date)) throw new ContractInactiveException(date);
        var release = _releases.First(r => r.Id == releaseId) ?? throw new ReleaseNotFoundException(releaseId);
        bool isFirstPublication = !release.IsPublishedAnywhere;
        release.MarkPublished(platformId, date);
        if (isFirstPublication) _contract?.AddNewRelease(releaseId, date);
    }

    public void AddTrackToRelease(Guid releaseId, Track track)
    {
        if (track.PerformerId != Id) throw new ArtistMismatchException(track);
        var release = _releases.First(r => r.Id == releaseId) ?? throw new ReleaseNotFoundException(releaseId);
        release.AddTrack(track.Id);
    }

    public void RemoveTrackFromRelease(Guid releaseId, Guid trackId)
    {
        var release = _releases.First(r => r.Id == releaseId) ?? throw new ReleaseNotFoundException(releaseId);
        release.RemoveTrack(trackId);
    }
    
    internal decimal CalculatePayments(decimal totalIncomeForRelease, DateOnly startDate, DateOnly endDate)
        => _contract?.CalculatePayments(totalIncomeForRelease, startDate, endDate) ?? 0;
}