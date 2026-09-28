using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artistlist")]
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
        var widths = header.Select((title, index) => Math.Max(title.Length, rows.Max(row => row[index].Length))).ToArray();
        PrintRow(header, widths);
        foreach (var row in rows) PrintRow(row, widths);
    }

    private static string FormatDate(DateOnly? date) => date is null ? "-" : $"{date.Value:dd.MM.yyyy}";
    private static void PrintRow(IReadOnlyList<string> cells, IReadOnlyList<int> widths) =>
        Console.WriteLine(string.Join("  ", cells.Select((cell, index) => cell.PadRight(widths[index]))).TrimEnd());

    public string Help => "Вывести список всех известных артистов: artist list";
}
