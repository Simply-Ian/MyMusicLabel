using MyMusicLabel.Domain.Entities;

namespace MyMusicLabel.Domain.Exceptions;

public class ReleaseNotFoundException(Guid? id = null) : ArgumentException
{
    public Guid? ReleaseId => id;
}

public class ReleaseNotReadyException(Release release) : Exception(
    release.TrackIds.Any()
        ? $"Релиз {release.Title} не готов к публикации"
        : $"релиз {release.Title} не содержит треков");
        
public class ReleaseAlreadyPublishedException : InvalidOperationException;

public class ReleaseNotPublishedException(Guid platformId) : InvalidOperationException
{
    public Guid PlatformId => platformId;
}