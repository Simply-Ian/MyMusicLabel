using FluentValidation;
using MyMusicLabel.Commands.Validators;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artist.create")]
internal class CreateArtistCommand(
    IArtistRepository artists,
    IArtistResolver aliases,
    IValidator<CreateArtistInput> validator
) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Укажите имя артиста: artist <короткое имя> create <Имя Фамилия>");
            return;
        }

        var input = new CreateArtistInput(args[1], string.Join(" ", args.Skip(3)));
        var validationResult = validator.Validate(input);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors) Console.WriteLine(error.ErrorMessage);
            return;
        }

        var artist = new Artist(Guid.NewGuid(), input.Name);
        artists.StoreArtist(artist);
        aliases.Occupy(input.Alias, artist.Id);
        Console.WriteLine($"Создан артист {artist.Name} с коротким именем \"{input.Alias}\", контракт не заключен");
    }

    public string Help => "Создать артиста: artist <короткое имя> create <Имя Фамилия>";
}
