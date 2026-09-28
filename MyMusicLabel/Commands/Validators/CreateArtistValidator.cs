using FluentValidation;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands.Validators;

internal record CreateArtistInput(string Alias, string Name);

internal class CreateArtistValidator : AbstractValidator<CreateArtistInput>
{
    public CreateArtistValidator(IArtistResolver aliases)
    {
        RuleFor(input => input.Name)
            .NotEmpty().WithMessage("Имя артиста не может быть пустым");
        RuleFor(input => input.Alias)
            .Must((_, alias) => !aliases.IsOccupied(alias))
            .WithMessage(input => $"Короткое имя \"{input.Alias}\" уже используется");
    }
}
