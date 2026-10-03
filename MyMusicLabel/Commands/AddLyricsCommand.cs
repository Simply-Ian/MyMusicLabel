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
    private readonly TrackProductionParser _parser = new(trackResolver, artistLookup, trackRepo);

    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length != 7)
        {
            Console.WriteLine(Usage);
            return;
        }

        var error = _parser.TryParseRequest(args, context.Today, out var request);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        error = _parser.TryProduce(
            "текст",
            request,
            () => productionService.ProduceTrack(request.Track.Id).AddLyrics(request.Author.Id)
        );
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        Console.WriteLine($"Трек \"{request.Track.Title}\": добавлен текст, текстовик {request.Author.Name}, " +
                          $"{_parser.DescribeTerms(request.Terms, "текст")}. ");
    }

    private const string Usage = "Укажите короткое имя трека, текстовика и условия оплаты: \n" +
                                 "\t track <короткое имя> addlyrics <короткое имя> for charge <процент> либо \n" +
                                 "\t track <короткое имя> addlyrics <короткое имя> for fixed <стоимость> либо \n";

    public string Help => "Добавить текст к треку: track <короткое имя трека> addlyrics <короткое имя текстовика> for " +
                          "charge <процент> - текстовик будет получать процент с доходов\n" +
                          "    track <короткое имя трека> addlyrics <короткое имя текстовика> for fixed <сумма> - " +
                          "текстовик будет получать фиксированную сумму за текст";
}
