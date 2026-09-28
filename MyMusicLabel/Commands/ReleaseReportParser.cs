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

        var error = DateParsing.TryParsePeriod(args, 4, 5, out var startDate, out var endDate);
        if (error is not null) return error;

        report = new ReleaseReport(artist.Releases[(int)releaseNumber - 1], artist, startDate, endDate);
        return null;
    }
}
