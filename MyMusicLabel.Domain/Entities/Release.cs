using MoreLinq;
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
            if (IsPublishedAnywhere) 
                throw new InvalidOperationException("Release is already published and thus track list cannot be edited");
            var trackSet = new HashSet<Guid>(value);
            if (trackSet.Count != trackIds.Count) 
                throw new ArgumentException("Release cannot contain the same track more than once");
            trackIds.Clear();
            value.ForEach(x => trackIds.Add(x));
        }
    }

    public void AddTrack(Guid trackId)
    {
        if (IsPublishedAnywhere)
            throw new InvalidOperationException("Release is already published at some platform and thus track list " +
                                                "cannot be edited");
        if (!trackIds.Contains(trackId)) trackIds.Add(trackId);
    }

    public void RemoveTrack(Guid trackId)
    {
        if (IsPublishedAnywhere)
            throw new InvalidOperationException("Release is already published at some platform and thus track list " +
                                                "cannot be edited");
        trackIds.Remove(trackId);
    }

    private readonly Dictionary<Guid, DateOnly> _platformsWherePublished = [];
    /// <summary>
    /// Хранит пары "Id платформы - дата публикации"
    /// </summary>
    public IReadOnlyDictionary<Guid, DateOnly> PlatformsWherePublished => _platformsWherePublished;

    internal void MarkPublished(Guid platformId, DateOnly publicationDate)
    {
        if (PlatformsWherePublished.ContainsKey(platformId)) 
            throw new InvalidOperationException($"Release is already published at platform with this Id: {platformId}");
        _platformsWherePublished.Add(platformId, publicationDate);
    }
    internal void MarkRevoked(Guid platformId)
    {
        if (!PlatformsWherePublished.ContainsKey(platformId))
            throw new InvalidOperationException("Cannot revoke the release which is not published on the specified " +
                                                $"platform: {platformId}");
        _platformsWherePublished.Remove(platformId);
    }
    
    public bool IsPublishedAnywhere => _platformsWherePublished.Count > 0;

    public string Title
    {
        get;
        set
        {
            if (IsPublishedAnywhere)
                throw new InvalidOperationException("Release is already published at some platform and thus cannot " +
                                                    "be renamed");
            field = value;
        }
    } = title;

    public Style Style { get; set; } = style;
}