using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("platform.list")]
internal class ListPlatformsCommand(IPlatformResolver platformResolver) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
        => Console.WriteLine(string.Join(", ", platformResolver.Aliases.Keys.OrderBy(alias => alias)));

    public string Help => "Вывести список доступных платформ: platform list";
}
