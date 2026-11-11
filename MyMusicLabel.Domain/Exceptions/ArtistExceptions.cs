using MyMusicLabel.Domain.Entities;

namespace MyMusicLabel.Domain.Exceptions;

public class AlreadyPerformsException : InvalidOperationException;

/* Конструктор по умолчанию нужен для обработки попытки добавить в релиз трек другого исполнителя. Ссылка на исполнителя
* уже содержится в самом объекте трека 
*/
public class ArtistMismatchException(Track? track) : InvalidOperationException
{
    public Track? Track => track;
    public ArtistMismatchException() : this(null)
    {
    }
}