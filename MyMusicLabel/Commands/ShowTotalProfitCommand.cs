using MyMusicLabel.Domain.Services;

namespace MyMusicLabel.Commands;

[CommandName("wallet.profit")]
internal class ShowTotalProfitCommand(WalletDomainService wallet) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        var error = WalletReportParser.TryParseRequest(args, "profit", out var startDate, out var endDate);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        var totalProfit = wallet.ComputeTotalProfitByPeriod(startDate, endDate);

        Console.WriteLine($"Общая прибыль за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}: " +
                          $"{totalProfit:0.##} руб.");
    }

    public string Help => "Показать общую прибыль лейбла за период: wallet total profit " +
                          "<дд.ММ.гггг> <дд.ММ.гггг>";
}
