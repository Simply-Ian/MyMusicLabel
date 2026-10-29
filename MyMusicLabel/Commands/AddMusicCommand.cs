using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("track.add.music")]
internal class AddMusicCommand(
    ITrackResolver trackResolver,
    IArtistLookup artistLookup,
    ITrackRepository trackRepo,
    TrackProductionDomainService productionService
) : ICommand
{
    private readonly TrackProductionParser _parser = new(trackResolver, artistLookup, trackRepo);

    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length != 9 || !args[4].Equals("duration", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(Usage);
            return;
        }

        if (!int.TryParse(args[5], out var durationSeconds))
        {
            Console.WriteLine($"Не удалось прочитать длительность \"{args[5]}\". Укажите целое число секунд");
            return;
        }

        if (durationSeconds <= 0)
        {
            Console.WriteLine("Длительность трека должна быть положительной");
            return;
        }

        var error = _parser.TryParseRequest(args, context.Today, out var request);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        error = _parser.TryProduce(
            "музыку",
            request,
            () => productionService.ProduceTrack(request.Track.Id).AddMusic(request.Author.Id, durationSeconds)
        );
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        var duration = TimeSpan.FromSeconds(durationSeconds);
        Console.WriteLine($"Трек \"{request.Track.Title}\": добавлена музыка, композитор {request.Author.Name}, " +
                          $"длительность {duration.Minutes}:{duration.Seconds:00}, " +
                          $"{_parser.DescribeTerms(request.Terms, "музыку")}.");
    }

    private const string Usage = "Укажите короткое имя трека, композитора, длительность и условия оплаты: \n" +
                                 "\t track <короткое имя> add.music <короткое имя> duration <секунды> for charge " +
                                 "<процент> либо \n" +
                                 "\t track <короткое имя> add.music <короткое имя> duration <секунды> for fixed " +
                                 "<стоимость> либо \n";

    public string Help => "Добавить музыку к треку: track <короткое имя трека> add.music <короткое имя композитора> " +
                          "duration <секунды> for charge <процент> - композитор будет получать процент с доходов\n" +
                          "    track <короткое имя трека> add.music <короткое имя композитора> duration <секунды> " +
                          "for fixed <сумма> - композитор будет получать фиксированную сумму за музыку";
}
