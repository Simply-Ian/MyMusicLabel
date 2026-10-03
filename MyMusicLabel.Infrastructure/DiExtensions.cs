using Microsoft.Extensions.DependencyInjection;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Infrastructure.Repos;
using MyMusicLabel.Infrastructure.Services;

namespace MyMusicLabel.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection RegisterPlatformAdapters(this IServiceCollection services, Dictionary<string, Guid> aliases)
        => services
            .AddKeyedSingleton<IStreamingPlatformAppService>(
                aliases["spotify"], new FakeStreamingPlatformAppService(3m, 0.8)
            )
            .AddKeyedSingleton<IStreamingPlatformAppService>(
                aliases["yandex"], new FakeStreamingPlatformAppService(5m, 0.6)
            );

    public static IServiceCollection RegisterRepositories(this IServiceCollection services) =>
        services.AddSingleton<IArtistRepository, ArtistRepository>()
            .AddSingleton<ITrackRepository, TrackRepository>();
}