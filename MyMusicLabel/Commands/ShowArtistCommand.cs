using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artist")]
internal class ShowArtistCommand(
    IArtistLookup artistLookup,
    IPlatformResolver platformResolver,
    ITrackResolver trackResolver,
    ITrackRepository trackRepo
) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Укажите короткое имя артиста: artist <короткое имя>");
            return;
        }

        var alias = args[1];
        if (!artistLookup.TryGetArtist(alias, out var artist))
        {
            Console.WriteLine("Артист с указанным коротким именем не найден");
            return;
        }

        Console.WriteLine(string.Join("  ", new[]
        {
            $"Короткое имя: {alias}",
            $"Имя: {artist.Name}",
            $"Подписан на {context.Today:dd.MM.yyyy}: {(artist.IsSignedToDate(context.Today) ? "да" : "нет")}",
            $"Дата подписания: {FormatDate(artist.SignDate)}",
            $"Дата расторжения: {FormatDate(artist.DropDate)}"
        }));

        if (!artist.Releases.Any())
        {
            Console.WriteLine("У артиста нет релизов");
            return;
        }

        for (var index = 0; index < artist.Releases.Count; index++)
        {
            var release = artist.Releases[index];
            Console.WriteLine($"{index + 1}. {release.Title} ({release.Style})");
            if (release.IsPublishedAnywhere)
            {
                foreach (var platform in release.PlatformsWherePublished
                             .OrderBy(pair => platformResolver.AliasesById.GetValueOrDefault(pair.Key, pair.Key.ToString())))
                {
                    Console.WriteLine($"\t{platformResolver.Resolve(platform.Key)}: {FormatDate(platform.Value)}");
                }
            }
            else
            {
                Console.WriteLine("\tне опубликован");
            }

            if (!release.TrackIds.Any()) continue;
            Console.WriteLine("\tTracklist:");
            foreach (var track in trackRepo.GetTrackList(release.TrackIds))
            {
                var trackAlias = trackResolver.AliasesById.GetValueOrDefault(track.Id, track.Id.ToString());
                Console.WriteLine($"\t\t{trackAlias} {track.Title}");
            }
        }
    }

    private static string FormatDate(DateOnly? date) => date is null ? "-" : $"{date.Value:dd.MM.yyyy}";

    public string Help => "Вывести одного артиста: artist <короткое имя>";
}
