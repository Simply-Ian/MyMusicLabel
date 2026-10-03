namespace MyMusicLabel.Commands;

[CommandName("exit")]
internal class ExitCommand : ICommand
{
    public void Invoke(string[] args, CommandContext context) => context.ExitRequested = true;

    public string Help => "Закончить работу с программой";
}