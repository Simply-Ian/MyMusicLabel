using MyMusicLabel.Domain.Services;

namespace MyMusicLabel.Commands;

[CommandName("walletincome")]
internal class ShowTotalIncomeCommand(WalletDomainService wallet) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        var error = WalletReportParser.TryParseRequest(args, "income", out var startDate, out var endDate);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        var totalIncome = wallet.ComputeTotalIncomeByPeriod(startDate, endDate);

        Console.WriteLine($"Общий доход за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}: " +
                          $"{totalIncome:0.##} руб.");
    }

    public string Help => "Показать общий доход лейбла за период: wallet total income <дд.ММ.гггг> <дд.ММ.гггг>";
}
