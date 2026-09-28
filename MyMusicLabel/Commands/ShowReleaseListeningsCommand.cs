using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artistlistenings")]
internal class ShowReleaseListeningsCommand(
    IArtistLookup artistLookup,
    ITrackRepository trackRepo
) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length != 6)
        {
            Console.WriteLine("Укажите номер релиза и период: artist <короткое имя> listenings <номер релиза> " +
                              "<дд.ММ.гггг> <дд.ММ.гггг>");
            return;
        }

        if (!artistLookup.TryGetArtist(args[1], out var artist))
        {
            Console.WriteLine("Артист с указанным коротким именем не найден");
            return;
        }

        if (!uint.TryParse(args[3], out var releaseNumber) || releaseNumber == 0)
        {
            Console.WriteLine("Номер релиза должен быть положительным целым числом");
            return;
        }

        if (releaseNumber > artist.Releases.Count)
        {
            Console.WriteLine($"У артиста {artist.Name} нет релиза с номером {releaseNumber}");
            return;
        }

        var culture = CultureInfo.GetCultureInfo("ru-RU");
        if (!TryParseDate(args[4], culture, out var startDate))
        {
            Console.WriteLine($"Не удалось прочитать дату \"{args[4]}\". Укажите дату в формате дд.ММ.гггг");
            return;
        }

        if (!TryParseDate(args[5], culture, out var endDate))
        {
            Console.WriteLine($"Не удалось прочитать дату \"{args[5]}\". Укажите дату в формате дд.ММ.гггг");
            return;
        }

        if (startDate > endDate)
        {
            Console.WriteLine("Дата начала периода не может быть позже даты окончания");
            return;
        }

        var release = artist.Releases[(int)releaseNumber - 1];
        List<Track> tracks;
        try
        {
            tracks = trackRepo.GetTrackList(release.TrackIds).ToList();
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Релиз содержит треки, которые больше не существуют");
            return;
        }

        if (release.PlatformsWherePublished.Count == 0)
        {
            Console.WriteLine($"Релиз \"{release.Title}\" артиста {artist.Name} не опубликован ни на одной " +
                              $"платформе, прослушиваний за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}: 0");
            return;
        }

        var totalListenings = context.Services
            .GetKeyedServices<StreamingPlatformDomainService>(KeyedService.AnyKey)
            .SelectMany(platformService => tracks
                .Select(track => platformService.GetTrackListeningsInPeriod(track.Id, startDate, endDate).Sum()))
            .Sum();

        Console.WriteLine($"Прослушиваний релиза \"{release.Title}\" артиста {artist.Name} за период " +
                          $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}: {totalListenings}");
    }

    private static bool TryParseDate(string arg, CultureInfo culture, out DateOnly date)
    {
        try
        {
            date = DateOnly.Parse(arg, culture);
            return true;
        }
        catch (FormatException)
        {
            date = default;
            return false;
        }
    }

    public string Help => "Показать количество прослушиваний релиза на всех платформах за период: artist " +
                          "<короткое имя> listenings <номер релиза> <дд.ММ.гггг> <дд.ММ.гггг>";
}
