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

public class Release : BaseEntity
{
    private List<Guid> _trackIds = [];
    public IReadOnlyList<Guid> TrackIds
    {
        get => _trackIds;
        set {
            if (IsPublishedAnywhere) 
                throw new InvalidOperationException("Release is already published and thus track list cannot be edited");
            var trackSet = new HashSet<Guid>(value);
            if (trackSet.Count != _trackIds.Count) 
                throw new ArgumentException("Release cannot contain the same track more than once");
            _trackIds.Clear();
            value.ForEach(x => _trackIds.Add(x));
        }
    }

    public void AddTrack(Guid trackId)
    {
        if (!_trackIds.Contains(trackId)) _trackIds.Add(trackId);
    }

    public void RemoveTrack(Guid trackId) => _trackIds.Remove(trackId);

    private readonly HashSet<Guid> _platformsWherePublished = [];
    public IReadOnlySet<Guid> PlatformsWherePublished => _platformsWherePublished;
    
    public DateOnly? PublicationDate { get; private set; }

    public void MarkPublished(Guid platformId, DateOnly publicationDate)
    {
        if (PlatformsWherePublished.Contains(platformId)) 
            throw new InvalidOperationException($"Release is already published at platform with this Id: {platformId}");
        _platformsWherePublished.Add(platformId);
        PublicationDate = publicationDate;
    }
    public void MarkRevoked(Guid platformId)
    {
        if (!PlatformsWherePublished.Contains(platformId))
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
    }
    
    public Style Style { get; set; }

    public Release(Guid id, Style style, string title, List<Guid> trackIds) : base(id)
    {
        Style = style;
        Title = title;
        TrackIds = trackIds;
    }
}