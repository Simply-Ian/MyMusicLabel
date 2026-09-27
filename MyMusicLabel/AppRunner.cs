using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MyMusicLabel.Commands;

namespace MyMusicLabel;

public class AppRunner (IServiceProvider services)
{
    public void Run()
    {
        // Команды регаются как transient и контейнер резолвит их зависимости
        CommandContext context = new(services);
        while (!context.ExitRequested)
        {
            Console.Write("> ");
            var rawInput = Console.ReadLine();
            if (string.IsNullOrEmpty(rawInput)) continue;
            
            var key = _parseInputToCommandKey(rawInput, out var args);
            var command = services.GetKeyedService<ICommand>(key);
            if (command is null)
            {
                Console.WriteLine("Неизвестная команда");
                continue;
            }
            command.Invoke(args.ToArray(), context);
        }
    }

    private static string _parseInputToCommandKey(string rawInput, out List<string> args)
    {
        args = rawInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            
        StringBuilder commandKeyBuilder = new(args[0]);
        commandKeyBuilder.Append(args.Count switch
        {
            >= 3 => args[2],
            2 => "view",
            _ => ""
        });
            
        return commandKeyBuilder.ToString();
    }
}