using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Exceptions;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Infrastructure.Services.Logging;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artist.drop")]
internal class DropArtistCommand(IArtistLookup artistLookup, IArtistRepository artistRepo, ICustomLogger logger) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        var alias = args[1];
        if (!artistLookup.TryGetArtist(alias, out var artist))
        {
            Console.WriteLine("Артист с указанным коротким именем не найден");
            return;
        }

        if (!artist.IsSignedToDate(context.Today))
        {
            Console.WriteLine($"Контракт с артистом {artist.Name} не заключен");
            return;
        }

        try
        {
            artist.Drop(context.Today);
        }
        catch (InvalidOperationException exc)
        {
            Console.WriteLine($"Невозможно расторгнуть контракт с исполнителем {artist.Name} по причине: {exc.Message}");
            if (exc is ContractNotFulfilledException cnfExc) _handleNotFulfilled(cnfExc, artist);
            logger.LogWarning("Ошибка расторжения контракта с артистом {0}", exc, artist.Name);
            return;
        }

        artistRepo.StoreArtist(artist);
        logger.LogInfo("Контракт с артистом {0} расторгнут", artist.Name);
        Console.WriteLine($"Контракт с исполнителем {artist.Name} успешно расторгнут {context.Today:dd.MM.yyyy}");
    }

    private static void _handleNotFulfilled(ContractNotFulfilledException cnfExc, Artist artist)
    {
        var releasesList = cnfExc.PlannedReleases
            .Join(
                artist.Releases,
                x => x.Id,
                rel => rel.Id, 
                (releasePlan, rel) => (rel, releasePlan.Date)
            )
            .ToList();
        foreach (var releaseDate in releasesList)
            Console.WriteLine($"{releaseDate.rel.Title} - запланирован на {releaseDate.Date}");
    }

    public string Help => "Расторгнуть контракт с исполнителем: artist <короткое имя> drop";
}
