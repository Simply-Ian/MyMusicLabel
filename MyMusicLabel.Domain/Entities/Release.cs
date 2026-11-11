using MoreLinq;
using MyMusicLabel.Domain.Exceptions;

namespace MyMusicLabel.Domain.Entities;

public enum Style
{
    Classic,
    Pop,
    Folk,
    HipHop,
    Blues,
    Jazz,
    Rock,
    Electronics,
    AuthorSong
}

public class Release(Guid id, Style style, string title, List<Guid> trackIds) : BaseEntity(id)
{
    public IReadOnlyList<Guid> TrackIds
    {
        get => trackIds;
        set {
            if (IsPublishedAnywhere) throw new ReleaseAlreadyPublishedException();
            var trackSet = new HashSet<Guid>(value);
            if (trackSet.Count != trackIds.Count) 
                throw new ArgumentException("Release cannot contain the same track more than once");
            trackIds.Clear();
            value.ForEach(trackIds.Add);
        }
    }

    public void AddTrack(Guid trackId)
    {
        if (IsPublishedAnywhere) throw new ReleaseAlreadyPublishedException();
        if (!trackIds.Contains(trackId)) trackIds.Add(trackId);
    }

    public void RemoveTrack(Guid trackId)
    {
        if (IsPublishedAnywhere) throw new ReleaseAlreadyPublishedException();
        trackIds.Remove(trackId);
    }

    private readonly Dictionary<Guid, DateOnly> _platformsWherePublished = [];
    /// <summary>
    /// Хранит пары "Id платформы - дата публикации"
    /// </summary>
    public IReadOnlyDictionary<Guid, DateOnly> PlatformsWherePublished => _platformsWherePublished;

    internal void MarkPublished(Guid platformId, DateOnly publicationDate)
    {
        if (PlatformsWherePublished.ContainsKey(platformId)) throw new ReleaseAlreadyPublishedException();
        _platformsWherePublished.Add(platformId, publicationDate);
    }
    internal void MarkRevoked(Guid platformId)
    {
        if (!PlatformsWherePublished.ContainsKey(platformId)) throw new ReleaseNotPublishedException(platformId);
        _platformsWherePublished.Remove(platformId);
    }
    
    public bool IsPublishedAnywhere => _platformsWherePublished.Count > 0;

    public string Title
    {
        get;
        set
        {
            if (IsPublishedAnywhere) throw new ReleaseAlreadyPublishedException();
            field = value;
        }
    } = title;

    public Style Style { get; set; } = style;
}