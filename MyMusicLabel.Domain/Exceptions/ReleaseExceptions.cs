using MyMusicLabel.Domain.Entities;

namespace MyMusicLabel.Domain.Exceptions;

public class ReleaseNotFoundException(Guid? id = null) : InvalidOperationException
{
    public Guid? ReleaseId => id;
}

public class ReleaseNotReadyException(Release release) : Exception(
    release.TrackIds.Any()
        ? $"Релиз {release.Title} не готов к публикации"
        : $"релиз {release.Title} не содержит треков");
        
public class ReleaseAlreadyPublishedException : Exception;

public class ReleaseNotPublishedException(Guid platformId) : Exception
{
    public Guid PlatformId => platformId;
}