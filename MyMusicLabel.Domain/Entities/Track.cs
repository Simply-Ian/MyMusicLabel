using MyMusicLabel.Domain.Contracts;

namespace MyMusicLabel.Domain.Entities;

// Track - агрегат, а не сущность в составе Release, потому что может входить в разные релизы
public class Track (Guid id) : BaseEntity(id)
{
    public string Title { get; set; } = "";
    public bool HasLyrics { get; private set; }
    public bool HasMusic { get; private set; }
    public bool IsRecorded { get; private set; }
    public bool IsReadyForPublishing => IsRecorded && HasMusic;
    public TimeSpan Duration { get; private set; }
    private AuthorInfo? MusicAuthorInfo { get; set; }
    public Guid? ComposerId => MusicAuthorInfo?.ArtistId;
    private AuthorInfo? LyricsAuthorInfo { get; set; }
    public Guid? LyricistId => LyricsAuthorInfo?.ArtistId;
    public Guid? PerformerId { get; private set; }
    
    public void AddMusic(Guid authorId, OneTimeContract contract, TimeSpan duration)
    {
        if (HasMusic) throw new InvalidOperationException("Track already has music");
        MusicAuthorInfo = new AuthorInfo(contract, authorId);
        Duration = duration;
        HasMusic = true;
    }
    
    public void AddLyrics(Guid authorId, OneTimeContract contract)
    {
        if (HasLyrics) throw new InvalidOperationException("Track already has lyrics");
        LyricsAuthorInfo = new AuthorInfo(contract, authorId);
        HasLyrics = true;
    }

    public void MarkRecorded(Guid performerId)
    {
        if (!HasMusic) throw new InvalidOperationException("Cannot record a track for which music isn't written yet");
        IsRecorded = true;
        PerformerId = performerId;
    }
    
    public decimal CalculateLyricistRoyalty(decimal income, DateOnly startDate, DateOnly endDate) 
        => LyricsAuthorInfo?.Contract.CalculatePayment(income, startDate, endDate) ?? 0;
    
    public decimal CalculateComposerRoyalty(decimal income, DateOnly startDate, DateOnly endDate) 
        => MusicAuthorInfo?.Contract.CalculatePayment(income, startDate, endDate) 
           ?? throw new InvalidOperationException("Track doesn't have any music yet");
}

public record AuthorInfo (OneTimeContract Contract, Guid ArtistId);