using MyMusicLabel.Domain.Services;

namespace MyMusicLabel.Commands;

[CommandName("walletroyalties")]
internal class ShowTotalRoyaltiesCommand(WalletDomainService wallet) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        var error = WalletReportParser.TryParseRequest(args, "royalties", out var startDate, out var endDate);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        var totalRoyalties = wallet.ComputeTotalRoyaltyByPeriod(startDate, endDate);

        Console.WriteLine($"Общие роялти за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}: " +
                          $"{totalRoyalties:0.##} руб.");
    }

    public string Help => "Показать общие роялти за период: wallet total royalties " +
                          "<дд.ММ.гггг> <дд.ММ.гггг>";
}
