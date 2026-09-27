using Microsoft.Extensions.DependencyInjection;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;

namespace MyMusicLabel.Domain;

public static class DiExtensions
{
    public static IServiceCollection RegisterPlatformDomainServices(this IServiceCollection services, Dictionary<string, Guid> aliases)
    {
        foreach (var platformId in aliases.Values)
        {
            services.AddKeyedSingleton<StreamingPlatformDomainService>(
                platformId,
                (sp, key) => new StreamingPlatformDomainService(
                    sp.GetRequiredKeyedService<IStreamingPlatformAppService>((Guid)key!),
                    sp.GetRequiredService<ITrackRepository>(),
                    sp.GetRequiredService<IArtistRepository>()));
        }

        return services;
    }

    public static IServiceCollection RegisterDomainServices(this IServiceCollection services) =>
        services.AddSingleton<WalletDomainService>()
            .AddSingleton<TrackProductionDomainService>();
}