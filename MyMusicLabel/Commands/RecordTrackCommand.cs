using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("trackrecord")]
internal class RecordTrackCommand(
    ITrackResolver trackResolver,
    IArtistLookup artistLookup,
    ITrackRepository trackRepo,
    TrackProductionDomainService productionService
) : ICommand
{
    private readonly TrackProductionParser _parser = new(trackResolver, artistLookup, trackRepo);

    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Укажите короткое имя трека и исполнителя: \n" +
                              "\t track <короткое имя> record <короткое имя исполнителя>\n");
            return;
        }

        var error = _parser.TryResolve(args[1], args[3], out var track, out var performer);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        error = _parser.TryRun(
            "записать трек",
            () => productionService.RecordTrack(track.Id, performer.Id)
        );
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        Console.WriteLine($"Трек \"{track.Title}\": записан исполнителем {performer.Name}.");
    }

    public string Help => "Записать трек исполнителем: track <короткое имя> record <короткое имя исполнителя>";
}
