using Microsoft.Extensions.DependencyInjection;
using MyMusicLabel.Domain.Entities;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Services;

namespace MyMusicLabel.Commands;

[CommandName("artistrelease.listenings")]
internal class ShowReleaseListeningsCommand(
    IArtistLookup artistLookup,
    ITrackRepository trackRepo
) : ICommand
{
    private readonly ReleaseReportParser _parser = new(artistLookup);

    public void Invoke(string[] args, CommandContext context)
    {
        var error = _parser.TryParseRequest(args, "release.listenings", out var report);
        if (error is not null)
        {
            Console.WriteLine(error);
            return;
        }

        var release = report.Release;
        List<Track> tracks;
        try
        {
            tracks = trackRepo.GetTrackList(release.TrackIds).ToList();
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Релиз содержит треки, которые больше не существуют");
            return;
        }

        if (release.PlatformsWherePublished.Count == 0)
        {
            Console.WriteLine($"Релиз \"{release.Title}\" артиста {report.Author.Name} не опубликован ни на одной " +
                              $"платформе, прослушиваний за период {report.Period}: 0");
            return;
        }

        var totalListenings = context.Services
            .GetKeyedServices<StreamingPlatformDomainService>(KeyedService.AnyKey)
            .SelectMany(platformService => tracks
                .Select(track => platformService.GetTrackListeningsInPeriod(
                    track.Id, report.StartDate, report.EndDate).Sum()))
            .Sum();

        Console.WriteLine($"Прослушиваний релиза \"{release.Title}\" артиста {report.Author.Name} за период " +
                          $"{report.Period}: {totalListenings}");
    }

    public string Help => "Показать количество прослушиваний релиза на всех платформах за период: artist " +
                          "<короткое имя> release.listenings <номер релиза> <дд.ММ.гггг> <дд.ММ.гггг>";
}
