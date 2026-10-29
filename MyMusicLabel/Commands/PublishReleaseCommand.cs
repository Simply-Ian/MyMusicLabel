using Microsoft.Extensions.DependencyInjection;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artist.publish")]
internal class PublishReleaseCommand(
    IArtistLookup artistLookup,
    IPlatformResolver platformResolver
) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length != 6 || !args[4].Equals("at", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Укажите номер релиза и платформу: artist <короткое имя> publish <номер релиза> at " +
                              "<короткое имя платформы>");
            return;
        }

        if (!artistLookup.TryGetArtist(args[1], out var artist))
        {
            Console.WriteLine("Артист с указанным коротким именем не найден");
            return;
        }

        if (!uint.TryParse(args[3], out var releaseNumber) || releaseNumber == 0)
        {
            Console.WriteLine("Номер релиза должен быть положительным целым числом");
            return;
        }

        if (releaseNumber > artist.Releases.Count)
        {
            Console.WriteLine($"У артиста {artist.Name} нет релиза с номером {releaseNumber}");
            return;
        }

        if (!platformResolver.Aliases.TryGetValue(args[5], out var platformId))
        {
            Console.WriteLine($"Платформа с указанным коротким именем не найдена: {args[5]}");
            return;
        }

        var release = artist.Releases[(int)releaseNumber - 1];
        try
        {
            var platformService = context.Services.GetRequiredKeyedService<StreamingPlatformDomainService>(platformId);
            platformService.Publish(release, artist.Id, platformId, context.Today);
        }
        catch (Exception exc)
        {
            Console.WriteLine($"Невозможно опубликовать релиз: {exc.Message}");
            return;
        }

        Console.WriteLine($"Релиз \"{release.Title}\" артиста {artist.Name} опубликован на платформе {args[5]}. " +
                          $"Дата: {context.Today:dd.MM.yyyy}");
    }

    public string Help => "Опубликовать релиз артиста на платформе: artist <короткое имя> publish " +
                          "<номер релиза> at <короткое имя платформы>";
}
