using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Output;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("tracklist")]
internal class ListTracksCommand(
    ITrackRepository trackRepo,
    ITrackResolver trackResolver,
    IArtistResolver artistResolver
) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        var rows = trackRepo.GetAllTracks()
            .OrderBy(track => track.Title)
            .Select(
                track => new[]
                {
                    ResolveAlias(trackResolver.AliasesById, track.Id),
                    track.Title,
                    ResolveAlias(artistResolver.AliasesById, track.ComposerId),
                    ResolveAlias(artistResolver.AliasesById, track.LyricistId),
                    ResolveAlias(artistResolver.AliasesById, track.PerformerId)
                }
            )
            .ToList();
        if (!rows.Any())
        {
            Console.WriteLine("Треки не зарегистрированы");
            return;
        }

        var header = new[] { "Короткое имя", "Название", "Композитор", "Автор текста", "Исполнитель" };
        TablePrinter.Print(header, rows);
    }

    private static string ResolveAlias(IReadOnlyDictionary<Guid, string> aliases, Guid? id)
        => id is null ? "-" : aliases.GetValueOrDefault(id.Value, id.Value.ToString());

    public string Help => "Вывести список всех известных треков: track list";
}
