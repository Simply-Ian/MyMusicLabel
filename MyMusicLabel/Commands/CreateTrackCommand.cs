using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("track.create")]
internal class CreateTrackCommand(ITrackRepository trackRepo, ITrackResolver trackResolver) : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Укажите название трека: track <короткое имя> create <название>");
            return;
        }

        var alias = args[1];
        if (trackResolver.IsOccupied(alias))
        {
            Console.WriteLine($"Короткое имя \"{alias}\" уже используется");
            return;
        }

        var track = new Track(Guid.NewGuid()) { Title = string.Join(" ", args.Skip(3)) };
        trackRepo.StoreTrack(track);
        trackResolver.Occupy(alias, track.Id);
        Console.WriteLine($"Создан трек \"{track.Title}\" с коротким именем \"{alias}\" без музыки и текста. Трек пока не записан");
    }

    public string Help => "Создать трек: track <короткое имя> create <название>";
}
