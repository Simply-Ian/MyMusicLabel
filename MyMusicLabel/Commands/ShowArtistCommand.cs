using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artist")]
internal class ShowArtistCommand(
    IArtistResolver artistResolver,
    IArtistRepository artistRepo,
    IPlatformResolver platformResolver
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
        Artist artist;
        try
        {
            artist = artistRepo.GetArtist(artistResolver.Resolve(alias));
        }
        catch (KeyNotFoundException)
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
            if (!release.IsPublishedAnywhere)
            {
                Console.WriteLine("\tне опубликован");
                continue;
            }

            foreach (var platform in release.PlatformsWherePublished
                         .OrderBy(pair => platformResolver.AliasesById.GetValueOrDefault(pair.Key, pair.Key.ToString())))
            {
                Console.WriteLine($"\t{platformResolver.Resolve(platform.Key)}: {FormatDate(platform.Value)}");
            }
        }
    }

    private static string FormatDate(DateOnly? date) => date is null ? "-" : $"{date.Value:dd.MM.yyyy}";

    public string Help => "Вывести одного артиста: artist <короткое имя>";
}
