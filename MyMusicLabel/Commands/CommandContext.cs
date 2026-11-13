namespace MyMusicLabel.Commands;

internal class CommandContext(IServiceProvider services)
{
    public DateOnly Today = DateOnly.FromDateTime(DateTime.Now);
    public bool ExitRequested;
    // Нужен для запросов по ключу
    public IServiceProvider Services = services;
}