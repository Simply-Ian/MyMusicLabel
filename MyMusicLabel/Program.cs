using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMusicLabel;
using MyMusicLabel.Domain;
using MyMusicLabel.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

var aliases = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
{
    ["spotify"] = Guid.NewGuid(),
    ["yandex"] = Guid.NewGuid()
};

builder.Services
    .RegisterRepositories()
    .RegisterPlatformAdapters(aliases)
    .RegisterAppServices()
    .RegisterPlatformDomainServices(aliases)
    .RegisterDomainServices()
    .RegisterCommands();

using var app = builder.Build();
app.Start();
app.Services.GetRequiredService<AppRunner>().Run();
