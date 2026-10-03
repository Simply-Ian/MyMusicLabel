namespace MyMusicLabel.Commands;

internal class CommandContext(IServiceProvider services)
{
    public DateOnly Today = DateOnly.FromDateTime(DateTime.Now);
    public bool ExitRequested;
    public IServiceProvider Services = services;
}