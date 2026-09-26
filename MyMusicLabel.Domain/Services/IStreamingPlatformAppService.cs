using MyMusicLabel.Domain.Entities;

namespace MyMusicLabel.Domain.Services;

/// <summary>
/// Реализации этого интерфейса описывают чисто технические детали взаимодействия с API платформы. Вся бизнес-логика
/// находится в StreamingPlatform.
/// Если бы мы делали интеграции с настоящими стримингами, то креды для API хранились бы в реализациях
/// </summary>
public interface IStreamingPlatformAppService
{
    void Publish(Release release, Guid artistId, DateOnly date);
    bool AnyPublicationsPlannedAfter(DateOnly date, Guid artistId);
    IEnumerable<int> GetTrackListeningsInPeriod(Guid trackId, DateOnly startDate, DateOnly endDate);
    IEnumerable<decimal> GetTrackPaymentsInPeriod(Guid trackId, DateOnly startDate, DateOnly endDate);
    bool IsReleasePublished(Guid releaseId);
    void Revoke(Guid releaseId, Guid artistId);
}