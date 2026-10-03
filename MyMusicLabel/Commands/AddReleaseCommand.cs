using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artistaddrelease")]
internal class AddReleaseCommand(IArtistLookup artistLookup, IArtistRepository artistRepo) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length < 5)
        {
            Console.WriteLine("Укажите название и стиль релиза: artist <короткое имя> addrelease <название> <стиль>");
            return;
        }

        var alias = args[1];
        if (!artistLookup.TryGetArtist(alias, out var artist))
        {
            Console.WriteLine("Артист с указанным коротким именем не найден");
            return;
        }

        var styleArg = args[^1];
        var styleName = Enum.GetNames<Style>()
            .FirstOrDefault(name => name.Equals(styleArg, StringComparison.OrdinalIgnoreCase));
        if (styleName is null)
        {
            Console.WriteLine($"Неизвестный стиль \"{styleArg}\". Доступны: {string.Join(", ", Enum.GetNames<Style>())}");
            return;
        }

        var title = string.Join(" ", args[3..^1]);
        var release = new Release(Guid.NewGuid(), Enum.Parse<Style>(styleName), title, []);
        artist.AddRelease(release);
        artistRepo.StoreArtist(artist);
        Console.WriteLine($"Создан релиз \"{release.Title}\" в стиле {release.Style} для артиста {artist.Name}");
    }

    public string Help => "Добавить релиз артисту: artist <короткое имя> addrelease <название> <стиль>";
}
