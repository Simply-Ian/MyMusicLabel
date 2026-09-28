namespace MyMusicLabel.Services;

public interface ITrackResolver
{
    bool IsOccupied(string alias);
    void Occupy(string alias, Guid trackId);

    Guid Resolve(string alias);

    IReadOnlyDictionary<string, Guid> Aliases { get; }
}
