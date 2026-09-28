using System.Globalization;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

internal class ReleaseReportParser(IArtistLookup artistLookup)
{
    internal record ReleaseReport(Release Release, Artist Author, DateOnly StartDate, DateOnly EndDate)
    {
        public string Period => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";
    }

    public string? TryParseRequest(IReadOnlyList<string> args, string command, out ReleaseReport report)
    {
        report = null!;
        if (args.Count != 6)
            return $"Укажите номер релиза и период: artist <короткое имя> {command} <номер релиза> " +
                   "<дд.ММ.гггг> <дд.ММ.гггг>";

        if (!artistLookup.TryGetArtist(args[1], out var artist))
            return "Артист с указанным коротким именем не найден";

        if (!uint.TryParse(args[3], out var releaseNumber) || releaseNumber == 0)
            return "Номер релиза должен быть положительным целым числом";

        if (releaseNumber > artist.Releases.Count)
            return $"У артиста {artist.Name} нет релиза с номером {releaseNumber}";

        var culture = CultureInfo.GetCultureInfo("ru-RU");
        if (!TryParseDate(args[4], culture, out var startDate))
            return $"Не удалось прочитать дату \"{args[4]}\". Укажите дату в формате дд.ММ.гггг";

        if (!TryParseDate(args[5], culture, out var endDate))
            return $"Не удалось прочитать дату \"{args[5]}\". Укажите дату в формате дд.ММ.гггг";

        if (startDate > endDate)
            return "Дата начала периода не может быть позже даты окончания";

        report = new ReleaseReport(artist.Releases[(int)releaseNumber - 1], artist, startDate, endDate);
        return null;
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
}
