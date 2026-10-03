using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artistsign")]
internal class SignArtistCommand(IArtistLookup artistLookup, IArtistRepository artistRepo) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length < 6)
        {
            Console.WriteLine("Недостаточно аргументов у команды");
            return;
        }
        var alias = args[1];
        if (!artistLookup.TryGetArtist(alias, out var artist))
        {
            Console.WriteLine("Артист с указанным коротким именем не найден");
            return;
        }

        var contractType = args[3];
        try
        {
            var releasesNum = uint.Parse(args[5]);
            if (contractType == "charge")
            {
                decimal chargePercents = decimal.Parse(args[4]);
                artist.SignPerformerForCharge(chargePercents / 100, context.Today, releasesNum);
                Console.WriteLine($"Заключен контракт на выпуск {releasesNum} релизов с исполнителем {artist.Name}. " +
                                  $"Величина отчислений: {chargePercents}% от доходов. Дата подписания: {context.Today.ToString("dd.MM.yyyy")}");
            }
            else if (contractType == "fixed")
            {
                decimal price = decimal.Parse(args[4]);
                artist.SignPerformerForFixedSum(price, context.Today, releasesNum);
                Console.WriteLine($"Заключен контракт на выпуск {releasesNum} релизов с исполнителем {artist.Name}. " +
                                  $"Величина отчислений: {price} руб. за первую публикацию релиза. Дата подписания: {context.Today.ToString("dd.MM.yyyy")}");
            }
            else
            {
                Console.WriteLine("Неизвестный тип контракта. Доступны: fixed, charge");
                return;
            }
        }
        catch (FormatException exc)
        {
            Console.WriteLine($"Указано некорректное числовое значение");
            return;
        }
        artistRepo.StoreArtist(artist);
    }

    public string Help => "заключить контракт с исполнителем на выпуск определенного количества релизов. Использование:\n" +
                          "    artist <короткое имя> sign charge <процентная ставка> <количество релизов> - исполнитель " +
                          "будет получать процент с доходов от релиза\n" +
                          "    artist <короткое имя> sign fixed <сумма выплаты> <количество релизов> - исполнитель будет " +
                          "получать фиксированную сумму за первую публикацию релиза";
}