using System.Globalization;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("trackaddlyrics")]
internal class AddLyricsCommand(
    ITrackResolver trackResolver,
    IArtistLookup artistLookup,
    ITrackRepository trackRepo,
    TrackProductionDomainService productionService
) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length != 7 || !args[4].Equals("for", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Укажите короткое имя трека, текстовика и условия оплаты: \n" +
                              "\t track <короткое имя> addlyrics <короткое имя> for charge <процент> либо \n" +
                              "\t track <короткое имя> addlyrics <короткое имя> for fixed <стоимость> либо \n");
            return;
        }

        Guid trackId;
        try
        {
            trackId = trackResolver.Resolve(args[1]);
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Трек с указанным коротким именем не найден");
            return;
        }

        if (!artistLookup.TryGetArtist(args[3], out var lyricist))
        {
            Console.WriteLine("Артист с указанным коротким именем не найден");
            return;
        }

        var contractType = args[5];
        var isCharge = contractType.Equals("charge", StringComparison.OrdinalIgnoreCase);
        if (!isCharge && !contractType.Equals("fixed", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Неизвестный тип контракта. Доступны: charge, fixed");
            return;
        }

        if (!decimal.TryParse(args[6], NumberStyles.Number, CultureInfo.CurrentCulture, out var value))
        {
            Console.WriteLine($"Не удалось прочитать числовое значение \"{args[6]}\"");
            return;
        }

        if (!isCharge && value < 0)
        {
            Console.WriteLine("Сумма выплаты не может быть отрицательной");
            return;
        }
        
        try
        {
            if (isCharge) 
                productionService
                    .ProduceTrack(trackId)
                    .AddLyrics(lyricist.Id)
                    .ForCharge(value / 100, context.Today);
            else 
                productionService
                    .ProduceTrack(trackId)
                    .AddLyrics(lyricist.Id)
                    .ForFixedSum(value, context.Today);
        }
        catch (InvalidOperationException exc)
        {
            Console.WriteLine($"Невозможно добавить текст: {exc.Message}");
            return;
        }

        var conditions = isCharge
            ? $"отчисления {value}% от дохода"
            : $"выплата {value} руб. за текст";
        var track = trackRepo.GetTrack(trackId);
        Console.WriteLine($"Трек \"{track.Title}\": добавлен текст, текстовик {lyricist.Name}, {conditions}. ");
    }

    public string Help => "Добавить текст к треку: track <короткое имя трека> addlyrics <короткое имя текстовика> for " +
                          "charge <процент> - текстовик будет получать процент с доходов\n" +
                          "    track <короткое имя трека> addlyrics <короткое имя текстовика> for fixed <сумма> - " +
                          "текстовик будет получать фиксированную сумму за текст";
}
