using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Output;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artist.list")]
internal class ListArtistsCommand(IArtistRepository artistRepo, IArtistResolver artistResolver) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        var rows = artistResolver.Aliases
            .Join(
                artistRepo.GetAllArtists(), 
                pair => pair.Value, 
                artist => artist.Id, 
                (pair, artist) => (Alias: pair.Key, Artist: artist)
            )
            .OrderBy(row => row.Artist.SignDate)
            .Select(
                row => new[]
                {
                    row.Alias, 
                    row.Artist.Name, 
                    row.Artist.IsSignedToDate(context.Today) ? "да" : "нет", 
                    FormatDate(row.Artist.SignDate), 
                    FormatDate(row.Artist.DropDate)
                }
            )
            .ToList();
        if (!rows.Any())
        {
            Console.WriteLine("Артисты не зарегистрированы");
            return;
        }

        var header = new[]
        {
            "Короткое имя", "Имя", $"Подписан на {context.Today:dd.MM.yyyy}", "Дата подписания", "Дата расторжения"
        };
        TablePrinter.Print(header, rows);
    }

    private static string FormatDate(DateOnly? date) => date is null ? "-" : $"{date.Value:dd.MM.yyyy}";

    public string Help => "Вывести список всех известных артистов: artist list";
}
