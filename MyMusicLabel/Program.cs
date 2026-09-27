using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMusicLabel.Domain.Repos;
using MyMusicLabel.Domain.Services;
using MyMusicLabel.Infrastructure.Repos;
using MyMusicLabel.Infrastructure.Services;
using MyMusicLabel.Services;

var builder = Host.CreateApplicationBuilder(args);

var aliases = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
{
    ["spotify"] = Guid.NewGuid(),
    ["yandex"] = Guid.NewGuid()
};

builder.Services.AddSingleton<IArtistRepository, ArtistRepository>();
builder.Services.AddSingleton<ITrackRepository, TrackRepository>();

builder.Services.AddKeyedSingleton<IStreamingPlatformAppService>(
    aliases["spotify"], new FakeStreamingPlatformAppService(3m, 0.8));
builder.Services.AddKeyedSingleton<IStreamingPlatformAppService>(
    aliases["yandex"], new FakeStreamingPlatformAppService(5m, 0.6));

foreach (var platformId in aliases.Values)
{
    builder.Services.AddKeyedSingleton<StreamingPlatformDomainService>(
        platformId,
        (sp, key) => new StreamingPlatformDomainService(
            sp.GetRequiredKeyedService<IStreamingPlatformAppService>((Guid)key!),
            sp.GetRequiredService<ITrackRepository>(),
            sp.GetRequiredService<IArtistRepository>()));
}

builder.Services.AddSingleton<WalletDomainService>();
builder.Services.AddSingleton<TrackProductionDomainService>();
builder.Services.AddSingleton<AppRunner>();

using var app = builder.Build();
app.Start();
app.Services.GetRequiredService<AppRunner>().Run();
