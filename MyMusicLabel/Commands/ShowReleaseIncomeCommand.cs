using MyMusicLabel.Domain.Services;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artist.release.income")]
internal class ShowReleaseIncomeCommand(
    IArtistLookup artistLookup,
    WalletDomainService wallet
) : ICommand
{
    private readonly ReleaseReportParser _parser = new(artistLookup);

    public void Invoke(string[] args, CommandContext context)
    {
        var error = _parser.TryParseRequest(args, "release.income", out var report);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        var release = report.Release;
        if (release.PlatformsWherePublished.Count == 0)
        {
            Console.WriteLine($"Релиз \"{release.Title}\" артиста {report.Author.Name} не опубликован ни на одной " +
                              $"платформе, дохода за период {report.Period}: 0 руб.");
            return;
        }

        var totalIncome = wallet.ComputeIncomeForRelease(release, report.StartDate, report.EndDate);

        Console.WriteLine($"Доход релиза \"{release.Title}\" артиста {report.Author.Name} за период " +
                          $"{report.Period}: {totalIncome:0.##} руб.");
    }

    public string Help => "Показать суммарный доход релиза на всех платформах за период: artist " +
                          "<короткое имя> release.income <номер релиза> <дд.ММ.гггг> <дд.ММ.гггг>";
}
