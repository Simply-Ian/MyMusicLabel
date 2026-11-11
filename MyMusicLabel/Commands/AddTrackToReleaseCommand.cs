using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Exceptions;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artist.add.track")]
internal class AddTrackToReleaseCommand(
    IArtistLookup artistLookup,
    ITrackResolver trackResolver,
    ITrackRepository trackRepo,
    IArtistRepository artistRepo,
    IPlatformResolver platformResolver
) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length != 6 || !args[4].Equals("to", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Укажите короткое имя трека и номер релиза: artist <короткое имя> add.track " +
                              "<короткое имя трека> to <номер релиза>");
            return;
        }

        if (!artistLookup.TryGetArtist(args[1], out var artist))
        {
            Console.WriteLine("Артист с указанным коротким именем не найден");
            return;
        }

        if (!uint.TryParse(args[5], out var releaseNumber) || releaseNumber == 0)
        {
            Console.WriteLine("Номер релиза должен быть положительным целым числом");
            return;
        }

        if (releaseNumber > artist.Releases.Count)
        {
            Console.WriteLine($"У артиста {artist.Name} нет релиза с номером {releaseNumber}");
            return;
        }

        Track track;
        try
        {
            track = trackRepo.GetTrack(trackResolver.Resolve(args[3]));
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Трек с указанным коротким именем не найден");
            return;
        }

        var release = artist.Releases[(int)releaseNumber - 1];
        try
        {
            artist.AddTrackToRelease(release.Id, track);
        }
        catch (ReleaseNotFoundException)
        {
            Console.WriteLine($"Указанный релиз не найден");
            return;
        }
        catch (ReleaseAlreadyPublishedException)
        {
            var platforms = release.PlatformsWherePublished
                .Select(pair => platformResolver.Resolve(pair.Key));
            Console.WriteLine($"Указанный релиз уже опубликован на платформах {string.Join(", ", platforms)} и не " +
                              "может быть отредактирован");
            return;
        }
        catch (ArtistMismatchException exc)
        {
            Console.WriteLine($"Трек {exc.Track} уже исполняется артистом {exc.Track!.PerformerId}");
            return;
        }

        artistRepo.StoreArtist(artist);
    }

    public string Help => "Добавить трек в релиз: artist <короткое имя> add.track <короткое имя трека> to " +
                          "<номер релиза>";
}
