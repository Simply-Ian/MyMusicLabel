using MyMusicLabel.Domain.Services;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artist.royalty")]
internal class ShowRoyaltyCommand(
    IArtistLookup artistLookup,
    WalletDomainService wallet
) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length != 5)
        {
            Console.WriteLine("Укажите период: artist <короткое имя> royalty <дд.ММ.гггг> <дд.ММ.гггг>");
            return;
        }

        if (!artistLookup.TryGetArtist(args[1], out var artist))
        {
            Console.WriteLine("Артист с указанным коротким именем не найден");
            return;
        }

        var error = DateParsing.TryParsePeriod(args, 3, 4, out var startDate, out var endDate);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        decimal totalRoyalty;
        try
        {
            totalRoyalty = wallet.ComputeRoyaltyToArtist(artist.Id, startDate, endDate);
        }
        // TODO: ввести типы доменных исключений для разных ситуаций
        catch (InvalidOperationException exc)
        {
            Console.WriteLine($"Не удалось рассчитать роялти: {exc.Message}");
            return;
        }

        Console.WriteLine($"Роялти артиста \"{artist.Name}\" за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}: " +
                          $"{totalRoyalty:0.##} руб.");
    }

    public string Help => "Показать сумму роялти артиста за период: artist <короткое имя> royalty " +
                          "<дд.ММ.гггг> <дд.ММ.гггг>";
}
