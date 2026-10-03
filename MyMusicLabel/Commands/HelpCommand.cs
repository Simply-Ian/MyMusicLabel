using Microsoft.Extensions.DependencyInjection;
using MoreLinq;

namespace MyMusicLabel.Commands;

[CommandName("help")]
internal class HelpCommand : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        var allCommands = context.Services.GetKeyedServices<ICommand>(KeyedService.AnyKey);
        foreach (var command in allCommands) Console.WriteLine($"{command.Name} - {command.Help}");
    }

    public string Help => "Вывести справку по командам";
}