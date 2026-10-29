using Microsoft.Extensions.DependencyInjection;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Output;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("tracklistenings")]
internal class TrackListeningsCommand(
    ITrackResolver trackResolver,
    ITrackRepository trackRepo,
    IPlatformResolver platformResolver
) : ICommand
{
    private static readonly char[] SpinnerFrames = ['\\', '|', '/', '-'];

    public void Invoke(string[] args, CommandContext context)
    {
        var platformId = Guid.Empty;
        DateOnly startDate = DateOnly.MinValue, endDate = DateOnly.MinValue;
        if (!_argsAreValid(args, ref platformId,  ref startDate, ref endDate)) return;
        var platform = context.Services.GetRequiredKeyedService<IStreamingPlatformAppService>(platformId);
        Track track;
        try
        {
            track = trackRepo.GetTrack(trackResolver.Resolve(args[1]));
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Трек с указанным коротким именем не найден");
            return;
        }
        
        bool completed = false, cancelled = false;
        Exception? taskError = null;
        IReadOnlyList<int> listenings = [];
        platform.GetTrackListeningsCompleted += OnCompleted;
        try
        {
            platform.GetTrackListeningsInPeriodAsync(track.Id, startDate, endDate);
            _waitWithIndicator(in completed, platform);
        }
        finally // Смысл try...finally без catch в том, что finally {} выполнится до того, как исключение развернет стек
        {
            platform.GetTrackListeningsCompleted -= OnCompleted;
        }

        _handleResponse(cancelled, taskError, listenings, endDate);

        void OnCompleted (object sender, IStreamingPlatformAppService.GetTrackListeningsCompletedArgs args)
        {
            taskError = args.Error;
            cancelled = args.Cancelled;
            listenings = args.Listenings?.ToList() ?? [];
            completed = true;
        }
    }

    private bool _argsAreValid(string[] args, ref Guid platformId, ref DateOnly startDate, ref DateOnly endDate)
    {
        if (args.Length != 6)
        {
            Console.WriteLine(Usage);
            return false;
        }

        if (!trackResolver.IsOccupied(args[1]))
        {
            Console.WriteLine("Трек с указанным коротким именем не найден");
            return false;
        }

        if (!platformResolver.Aliases.TryGetValue(args[3], out platformId))
        {
            Console.WriteLine($"Платформа с указанным коротким именем не найдена: {args[3]}");
            return false;
        }

        var error = DateParsing.TryParsePeriod(args, 4, 5, out startDate, out endDate);
        if (error is not null)
        {
            Console.WriteLine(error);
            return false;
        }

        return true;
    }

    private static void _waitWithIndicator(
        in bool completed,
        IStreamingPlatformAppService platform
    )
    {
        var frameIndex = 0;
        while (!completed)
        {
            if (!Console.IsInputRedirected && Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.C)
                {
                    platform.GetTrackListeningsInPeriodAsyncCancel();
                    break;
                }
            }
            _drawSpinner(frameIndex);
            frameIndex++;
        }
        Console.Write(_cursorBack);
        Console.Out.Flush();
    }

    private static void _drawSpinner(int frameIndex)
    {
        Console.Write(SpinnerFrames[frameIndex % SpinnerFrames.Length]);
        Console.Out.Flush();
        Console.Write(_cursorBack);
        Thread.Sleep(100);
    }
    
    private static void _handleResponse(bool cancelled, Exception? taskError, IReadOnlyList<int> listenings, DateOnly endDate)
    {
        if (cancelled) return;
        if (taskError is not null)
        {
            Console.WriteLine($"Не удалось получить прослушивания: {taskError.Message}");
            return;
        }
        if (!listenings.Any() || listenings.Count == 0)
        {
            Console.WriteLine("За указанный период прослушиваний нет");
            return;
        }
        
        var firstDate = endDate.AddDays(1 - listenings.Count);
        var rows = listenings
            .Select((count, index) => new[]
            {
                firstDate.AddDays(index).ToString("dd.MM.yyyy"),
                count.ToString()
            })
            .ToList();
        TablePrinter.Print(new[] { "Дата", "Прослушивания" }, rows);
    }

    private const string Usage = "Укажите платформу и период: track <короткое имя трека> listenings " +
                                 "<короткое имя платформы> <дд.ММ.гггг> <дд.ММ.гггг>";

    private const string? _cursorBack = "\e[1D";

    public string Help => "Показать прослушивания трека на платформе за период: track <короткое имя> listenings " +
                          "<короткое имя платформы> <дд.ММ.гггг> <дд.ММ.гггг>";
}
