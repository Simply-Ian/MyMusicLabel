using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artistdrop")]
internal class DropArtistCommand(IArtistLookup artistLookup, IArtistRepository artistRepo) : ICommand
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
        // TODO: ввести типы доменных исключений для разных ситуаций
        catch (InvalidOperationException exc)
        {
            Console.WriteLine($"Невозможно расторгнуть контракт с исполнителем {artist.Name}: {exc.Message}");
            return;
        }

        artistRepo.StoreArtist(artist);
        Console.WriteLine($"Контракт с исполнителем {artist.Name} успешно расторгнут {context.Today:dd.MM.yyyy}");
    }

    public string Help => "Расторгнуть контракт с исполнителем: artist <короткое имя> drop";
}
