using System.Globalization;

namespace MyMusicLabel.Commands;

[CommandName("today")]
internal class TodayCommand : ICommand
{
    public void Invoke(string[] args, CommandContext context)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Укажите дату в формате дд.ММ.гггг");
            return;
        }

        var culture = CultureInfo.GetCultureInfo("ru-RU");
        try
        {
            context.Today = DateOnly.Parse(args[1], culture);
        }
        catch (FormatException)
        {
            Console.WriteLine($"Не удалось прочитать дату \"{args[1]}\". Укажите дату в формате дд.ММ.гггг");
            return;
        }

        Console.WriteLine($"Сегодняшняя дата: {context.Today:dd.MM.yyyy}");
    }

    public string Help => "Задать сегодняшнюю дату в формате дд.ММ.гггг";
}
